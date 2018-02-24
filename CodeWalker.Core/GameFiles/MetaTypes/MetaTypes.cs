using SharpDX;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml;

using TC = System.ComponentModel.TypeConverterAttribute;
using EXP = System.ComponentModel.ExpandableObjectConverter;
using CodeWalker.World;

namespace CodeWalker.GameFiles
{

    //this is a helper class for parsing the data.
    public static class MetaTypes
    {

        public static Dictionary<uint, MetaEnumInfo> EnumDict = new Dictionary<uint, MetaEnumInfo>();
        public static Dictionary<uint, MetaStructureInfo> StructDict = new Dictionary<uint, MetaStructureInfo>();

        public static void Clear()
        {
            StructDict.Clear();
        }

        public static void EnsureMetaTypes(Meta meta)
        {

            if (meta.EnumInfos != null)
            {
                foreach (MetaEnumInfo mei in meta.EnumInfos)
                {
                    if (!EnumDict.ContainsKey(mei.EnumKey))
                    {
                        EnumDict.Add(mei.EnumKey, mei);
                    }
                    else
                    {
                        MetaEnumInfo oldei = EnumDict[mei.EnumKey];
                        if (!CompareMetaEnumInfos(oldei, mei))
                        {
                        }
                    }
                }
            }

            if (meta.StructureInfos != null)
            {
                foreach (MetaStructureInfo msi in meta.StructureInfos)
                {
                    if (!StructDict.ContainsKey(msi.StructureKey))
                    {
                        StructDict.Add(msi.StructureKey, msi);
                    }
                    else
                    {
                        MetaStructureInfo oldsi = StructDict[msi.StructureKey];
                        if (!CompareMetaStructureInfos(oldsi, msi))
                        {
                        }
                    }
                }
            }

        }

        public static bool CompareMetaEnumInfos(MetaEnumInfo a, MetaEnumInfo b)
        {
            //returns true if they are the same.

            if (a.Entries.Length != b.Entries.Length)
            {
                return false;
            }

            for (int i = 0; i < a.Entries.Length; i++)
            {
                if ((a.Entries[i].EntryNameHash != b.Entries[i].EntryNameHash) ||
                    (a.Entries[i].EntryValue != b.Entries[i].EntryValue))
                {
                    return false;
                }
            }

            return true;
        }
        public static bool CompareMetaStructureInfos(MetaStructureInfo a, MetaStructureInfo b)
        {
            //returns true if they are the same.

            if (a.Entries.Length != b.Entries.Length)
            {
                return false;
            }

            for (int i = 0; i < a.Entries.Length; i++)
            {
                if ((a.Entries[i].EntryNameHash != b.Entries[i].EntryNameHash) ||
                    (a.Entries[i].DataOffset != b.Entries[i].DataOffset) ||
                    (a.Entries[i].DataType != b.Entries[i].DataType))
                {
                    return false;
                }
            }

            return true;
        }


        public static string GetTypesString()
        {
            StringBuilder sbe = new StringBuilder();
            StringBuilder sbs = new StringBuilder();

            sbe.AppendLine("//Enum infos");
            sbs.AppendLine("//Struct infos");


            foreach (var kvp in EnumDict)
            {
                var mei = kvp.Value;
                string name = GetSafeName(mei.EnumNameHash, mei.EnumKey);
                sbe.AppendLine("public enum " + name + " //Key:" + mei.EnumKey.ToString());
                sbe.AppendLine("{");
                foreach (var entry in mei.Entries)
                {
                    string eename = GetSafeName(entry.EntryNameHash, (uint)entry.EntryValue);
                    sbe.AppendFormat("   {0} = {1},", eename, entry.EntryValue);
                    sbe.AppendLine();
                }
                sbe.AppendLine("}");
                sbe.AppendLine();
            }

            foreach (var kvp in StructDict)
            {
                var msi = kvp.Value;
                string name = GetSafeName(msi.StructureNameHash, msi.StructureKey);
                sbs.AppendLine("public struct " + name + " //" + msi.StructureSize.ToString() + " bytes, Key:" + msi.StructureKey.ToString());
                sbs.AppendLine("{");
                for (int i = 0; i < msi.Entries.Length; i++)
                {
                    var entry = msi.Entries[i];

                    if ((entry.DataOffset == 0) && (entry.EntryNameHash == MetaName.ARRAYINFO)) //referred to by array
                    {
                    }
                    else
                    {
                        string sename = GetSafeName(entry.EntryNameHash, (uint)entry.ReferenceKey);
                        string fmt = "   public {0} {1}; //{2}   {3}";

                        if (entry.DataType == MetaStructureEntryDataType.Array)
                        {
                            if (entry.ReferenceTypeIndex >= msi.Entries.Length)
                            {
                            }
                            else
                            {
                                var structentry = msi.Entries[entry.ReferenceTypeIndex];
                                var typename = "Array_" + MetaStructureEntryDataTypes.GetCSharpTypeName(structentry.DataType);
                                sbs.AppendFormat(fmt, typename, sename, entry.DataOffset, entry.ToString() + "  {" + structentry.ToString() + "}");
                                sbs.AppendLine();
                            }
                        }
                        else if (entry.DataType == MetaStructureEntryDataType.Structure)
                        {
                            var typename = GetSafeName(entry.ReferenceKey, (uint)entry.ReferenceTypeIndex);
                            sbs.AppendFormat(fmt, typename, sename, entry.DataOffset, entry.ToString());
                            sbs.AppendLine();
                        }
                        else
                        {
                            var typename = MetaStructureEntryDataTypes.GetCSharpTypeName(entry.DataType);
                            sbs.AppendFormat(fmt, typename, sename, entry.DataOffset, entry);
                            sbs.AppendLine();
                        }

                    }


                }
                sbs.AppendLine("}");
                sbs.AppendLine();
            }


            sbe.AppendLine();
            sbe.AppendLine();
            sbe.AppendLine();
            sbe.AppendLine();
            sbe.AppendLine();
            sbe.Append(sbs.ToString());

            string result = sbe.ToString();

            return result;
        }


        public static string GetTypesInitString(Meta meta)
        {
            StringBuilder sb = new StringBuilder();

            foreach (var si in meta.StructureInfos)
            {
                sb.AppendFormat("return new MetaStructureInfo(MetaName.{0}, {1}, {2}, {3},", si.StructureNameHash, si.StructureKey, si.Unknown_8h, si.StructureSize);
                sb.AppendLine();
                for (int i = 0; i < si.Entries.Length; i++)
                {
                    var e = si.Entries[i];
                    string refkey = "0";
                    if (e.ReferenceKey != 0)
                    {
                        refkey = "MetaName." + e.ReferenceKey.ToString();
                    }
                    sb.AppendFormat("new MetaStructureEntryInfo_s(MetaName.{0}, {1}, MetaStructureEntryDataType.{2}, {3}, {4}, {5})", e.EntryNameHash, e.DataOffset, e.DataType, e.Unknown_9h, e.ReferenceTypeIndex, refkey);
                    if (i < si.Entries.Length - 1) sb.Append(",");
                    sb.AppendLine();
                }
                sb.AppendFormat(");");
                sb.AppendLine();
            }

            sb.AppendLine();

            foreach (var ei in meta.EnumInfos)
            {
                sb.AppendFormat("return new MetaEnumInfo(MetaName.{0}, {1},", ei.EnumNameHash, ei.EnumKey);
                sb.AppendLine();
                for (int i = 0; i < ei.Entries.Length; i++)
                {
                    var e = ei.Entries[i];
                    sb.AppendFormat("new MetaEnumEntryInfo_s(MetaName.{0}, {1})", e.EntryNameHash, e.EntryValue);
                    if (i < ei.Entries.Length - 1) sb.Append(",");
                    sb.AppendLine();
                }
                sb.AppendFormat(");");
                sb.AppendLine();
            }


            string str = sb.ToString();
            return str;
        }


        public static MetaStructureInfo GetStructureInfo(MetaName name)
        {
            //to generate structinfos
            switch (name)
            {

                /* YMAP types */
                case MetaName.VECTOR3:
                    return new MetaStructureInfo(MetaName.VECTOR3, 2751397072, 512, 12,
                    new MetaStructureEntryInfo_s(MetaName.x, 0, MetaStructureEntryDataType.Float, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.y, 4, MetaStructureEntryDataType.Float, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.z, 8, MetaStructureEntryDataType.Float, 0, 0, 0)
                    );
                case MetaName.rage__fwInstancedMapData:
                    return new MetaStructureInfo(MetaName.rage__fwInstancedMapData, 1836780118, 768, 48,
                    new MetaStructureEntryInfo_s(MetaName.ImapLink, 8, MetaStructureEntryDataType.Hash, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.ARRAYINFO, 0, MetaStructureEntryDataType.Structure, 0, 0, MetaName.rage__fwPropInstanceListDef),
                    new MetaStructureEntryInfo_s(MetaName.PropInstanceList, 16, MetaStructureEntryDataType.Array, 0, 1, 0),
                    new MetaStructureEntryInfo_s(MetaName.ARRAYINFO, 0, MetaStructureEntryDataType.Structure, 0, 0, MetaName.rage__fwGrassInstanceListDef),
                    new MetaStructureEntryInfo_s(MetaName.GrassInstanceList, 32, MetaStructureEntryDataType.Array, 0, 3, 0)
                    );
                case MetaName.rage__fwGrassInstanceListDef:
                    return new MetaStructureInfo(MetaName.rage__fwGrassInstanceListDef, 941808164, 1024, 96,
                    new MetaStructureEntryInfo_s(MetaName.BatchAABB, 0, MetaStructureEntryDataType.Structure, 0, 0, MetaName.rage__spdAABB),
                    new MetaStructureEntryInfo_s(MetaName.ScaleRange, 32, MetaStructureEntryDataType.Float_XYZ, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.archetypeName, 48, MetaStructureEntryDataType.Hash, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.lodDist, 52, MetaStructureEntryDataType.UnsignedInt, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.LodFadeStartDist, 56, MetaStructureEntryDataType.Float, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.LodInstFadeRange, 60, MetaStructureEntryDataType.Float, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.OrientToTerrain, 64, MetaStructureEntryDataType.Float, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.ARRAYINFO, 0, MetaStructureEntryDataType.Structure, 0, 0, MetaName.rage__fwGrassInstanceListDef__InstanceData),
                    new MetaStructureEntryInfo_s(MetaName.InstanceList, 72, MetaStructureEntryDataType.Array, 36, 7, 0)
                    );
                case MetaName.rage__fwGrassInstanceListDef__InstanceData:
                    return new MetaStructureInfo(MetaName.rage__fwGrassInstanceListDef__InstanceData, 2740378365, 256, 16,
                    new MetaStructureEntryInfo_s(MetaName.ARRAYINFO, 0, MetaStructureEntryDataType.UnsignedShort, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.Position, 0, MetaStructureEntryDataType.ArrayOfBytes, 0, 0, (MetaName)3),
                    new MetaStructureEntryInfo_s(MetaName.NormalX, 6, MetaStructureEntryDataType.UnsignedByte, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.NormalY, 7, MetaStructureEntryDataType.UnsignedByte, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.ARRAYINFO, 0, MetaStructureEntryDataType.UnsignedByte, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.Color, 8, MetaStructureEntryDataType.ArrayOfBytes, 0, 4, (MetaName)3),
                    new MetaStructureEntryInfo_s(MetaName.Scale, 11, MetaStructureEntryDataType.UnsignedByte, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.Ao, 12, MetaStructureEntryDataType.UnsignedByte, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.ARRAYINFO, 0, MetaStructureEntryDataType.UnsignedByte, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.Pad, 13, MetaStructureEntryDataType.ArrayOfBytes, 0, 8, (MetaName)3)
                    );
                case MetaName.rage__spdAABB:
                    return new MetaStructureInfo(MetaName.rage__spdAABB, 1158138379, 1024, 32,
                    new MetaStructureEntryInfo_s(MetaName.min, 0, MetaStructureEntryDataType.Float_XYZW, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.max, 16, MetaStructureEntryDataType.Float_XYZW, 0, 0, 0)
                    );
                case MetaName.CLODLight:
                    return new MetaStructureInfo(MetaName.CLODLight, 2325189228, 768, 136,
                    new MetaStructureEntryInfo_s(MetaName.ARRAYINFO, 0, MetaStructureEntryDataType.Structure, 0, 0, MetaName.VECTOR3),
                    new MetaStructureEntryInfo_s(MetaName.direction, 8, MetaStructureEntryDataType.Array, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.ARRAYINFO, 0, MetaStructureEntryDataType.Float, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.falloff, 24, MetaStructureEntryDataType.Array, 0, 2, 0),
                    new MetaStructureEntryInfo_s(MetaName.ARRAYINFO, 0, MetaStructureEntryDataType.Float, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.falloffExponent, 40, MetaStructureEntryDataType.Array, 0, 4, 0),
                    new MetaStructureEntryInfo_s(MetaName.ARRAYINFO, 0, MetaStructureEntryDataType.UnsignedInt, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.timeAndStateFlags, 56, MetaStructureEntryDataType.Array, 0, 6, 0),
                    new MetaStructureEntryInfo_s(MetaName.ARRAYINFO, 0, MetaStructureEntryDataType.UnsignedInt, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.hash, 72, MetaStructureEntryDataType.Array, 0, 8, 0),
                    new MetaStructureEntryInfo_s(MetaName.ARRAYINFO, 0, MetaStructureEntryDataType.UnsignedByte, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.coneInnerAngle, 88, MetaStructureEntryDataType.Array, 0, 10, 0),
                    new MetaStructureEntryInfo_s(MetaName.ARRAYINFO, 0, MetaStructureEntryDataType.UnsignedByte, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.coneOuterAngleOrCapExt, 104, MetaStructureEntryDataType.Array, 0, 12, 0),
                    new MetaStructureEntryInfo_s(MetaName.ARRAYINFO, 0, MetaStructureEntryDataType.UnsignedByte, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.coronaIntensity, 120, MetaStructureEntryDataType.Array, 0, 14, 0)
                    );
                case MetaName.CDistantLODLight:
                    return new MetaStructureInfo(MetaName.CDistantLODLight, 2820908419, 768, 48,
                    new MetaStructureEntryInfo_s(MetaName.ARRAYINFO, 0, MetaStructureEntryDataType.Structure, 0, 0, MetaName.VECTOR3),
                    new MetaStructureEntryInfo_s(MetaName.position, 8, MetaStructureEntryDataType.Array, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.ARRAYINFO, 0, MetaStructureEntryDataType.UnsignedInt, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.RGBI, 24, MetaStructureEntryDataType.Array, 0, 2, 0),
                    new MetaStructureEntryInfo_s(MetaName.numStreetLights, 40, MetaStructureEntryDataType.UnsignedShort, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.category, 42, MetaStructureEntryDataType.UnsignedShort, 0, 0, 0)
                    );
                case MetaName.CBlockDesc:
                    return new MetaStructureInfo(MetaName.CBlockDesc, 2015795449, 768, 72,
                    new MetaStructureEntryInfo_s(MetaName.version, 0, MetaStructureEntryDataType.UnsignedInt, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.flags, 4, MetaStructureEntryDataType.UnsignedInt, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.name, 8, MetaStructureEntryDataType.CharPointer, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.exportedBy, 24, MetaStructureEntryDataType.CharPointer, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.owner, 40, MetaStructureEntryDataType.CharPointer, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.time, 56, MetaStructureEntryDataType.CharPointer, 0, 0, 0)
                    );
                case MetaName.CMapData:
                    return new MetaStructureInfo(MetaName.CMapData, 3448101671, 1024, 512,
                    new MetaStructureEntryInfo_s(MetaName.name, 8, MetaStructureEntryDataType.Hash, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.parent, 12, MetaStructureEntryDataType.Hash, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.flags, 16, MetaStructureEntryDataType.UnsignedInt, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.contentFlags, 20, MetaStructureEntryDataType.UnsignedInt, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.streamingExtentsMin, 32, MetaStructureEntryDataType.Float_XYZ, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.streamingExtentsMax, 48, MetaStructureEntryDataType.Float_XYZ, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.entitiesExtentsMin, 64, MetaStructureEntryDataType.Float_XYZ, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.entitiesExtentsMax, 80, MetaStructureEntryDataType.Float_XYZ, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.ARRAYINFO, 0, MetaStructureEntryDataType.StructurePointer, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.entities, 96, MetaStructureEntryDataType.Array, 0, 8, 0),
                    new MetaStructureEntryInfo_s(MetaName.ARRAYINFO, 0, MetaStructureEntryDataType.Structure, 0, 0, (MetaName)372253349),
                    new MetaStructureEntryInfo_s(MetaName.containerLods, 112, MetaStructureEntryDataType.Array, 0, 10, 0),
                    new MetaStructureEntryInfo_s(MetaName.ARRAYINFO, 0, MetaStructureEntryDataType.Structure, 0, 0, (MetaName)975711773/*.SectionUNKNOWN7*/),
                    new MetaStructureEntryInfo_s(MetaName.boxOccluders, 128, MetaStructureEntryDataType.Array, 4, 12, 0),
                    new MetaStructureEntryInfo_s(MetaName.ARRAYINFO, 0, MetaStructureEntryDataType.Structure, 0, 0, (MetaName)2741784237/*.SectionUNKNOWN5*/),
                    new MetaStructureEntryInfo_s(MetaName.occludeModels, 144, MetaStructureEntryDataType.Array, 4, 14, 0),
                    new MetaStructureEntryInfo_s(MetaName.ARRAYINFO, 0, MetaStructureEntryDataType.Hash, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.physicsDictionaries, 160, MetaStructureEntryDataType.Array, 0, 16, 0),
                    new MetaStructureEntryInfo_s(MetaName.instancedData, 176, MetaStructureEntryDataType.Structure, 0, 0, MetaName.rage__fwInstancedMapData),
                    new MetaStructureEntryInfo_s(MetaName.ARRAYINFO, 0, MetaStructureEntryDataType.Structure, 0, 0, MetaName.CTimeCycleModifier),
                    new MetaStructureEntryInfo_s(MetaName.timeCycleModifiers, 224, MetaStructureEntryDataType.Array, 0, 19, 0),
                    new MetaStructureEntryInfo_s(MetaName.ARRAYINFO, 0, MetaStructureEntryDataType.Structure, 0, 0, MetaName.CCarGen),
                    new MetaStructureEntryInfo_s(MetaName.carGenerators, 240, MetaStructureEntryDataType.Array, 0, 21, 0),
                    new MetaStructureEntryInfo_s(MetaName.LODLightsSOA, 256, MetaStructureEntryDataType.Structure, 0, 0, MetaName.CLODLight),
                    new MetaStructureEntryInfo_s(MetaName.DistantLODLightsSOA, 392, MetaStructureEntryDataType.Structure, 0, 0, MetaName.CDistantLODLight),
                    new MetaStructureEntryInfo_s(MetaName.block, 440, MetaStructureEntryDataType.Structure, 0, 0, MetaName.CBlockDesc)
                    );
                case MetaName.CEntityDef:
                    return new MetaStructureInfo(MetaName.CEntityDef, 1825799514, 1024, 128,
                    new MetaStructureEntryInfo_s(MetaName.archetypeName, 8, MetaStructureEntryDataType.Hash, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.flags, 12, MetaStructureEntryDataType.UnsignedInt, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.guid, 16, MetaStructureEntryDataType.UnsignedInt, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.position, 32, MetaStructureEntryDataType.Float_XYZ, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.rotation, 48, MetaStructureEntryDataType.Float_XYZW, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.scaleXY, 64, MetaStructureEntryDataType.Float, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.scaleZ, 68, MetaStructureEntryDataType.Float, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.parentIndex, 72, MetaStructureEntryDataType.SignedInt, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.lodDist, 76, MetaStructureEntryDataType.Float, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.childLodDist, 80, MetaStructureEntryDataType.Float, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.lodLevel, 84, MetaStructureEntryDataType.IntEnum, 0, 0, (MetaName)1264241711),
                    new MetaStructureEntryInfo_s(MetaName.numChildren, 88, MetaStructureEntryDataType.UnsignedInt, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.priorityLevel, 92, MetaStructureEntryDataType.IntEnum, 0, 0, (MetaName)648413703),
                    new MetaStructureEntryInfo_s(MetaName.ARRAYINFO, 0, MetaStructureEntryDataType.StructurePointer, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.extensions, 96, MetaStructureEntryDataType.Array, 0, 13, 0),
                    new MetaStructureEntryInfo_s(MetaName.ambientOcclusionMultiplier, 112, MetaStructureEntryDataType.SignedInt, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.artificialAmbientOcclusion, 116, MetaStructureEntryDataType.SignedInt, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.tintValue, 120, MetaStructureEntryDataType.UnsignedInt, 0, 0, 0)
                    );
                case MetaName.CMloInstanceDef:
                    return new MetaStructureInfo(MetaName.CMloInstanceDef, 2151576752, 1024, 160,
                    new MetaStructureEntryInfo_s(MetaName.archetypeName, 8, MetaStructureEntryDataType.Hash, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.flags, 12, MetaStructureEntryDataType.UnsignedInt, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.guid, 16, MetaStructureEntryDataType.UnsignedInt, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.position, 32, MetaStructureEntryDataType.Float_XYZ, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.rotation, 48, MetaStructureEntryDataType.Float_XYZW, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.scaleXY, 64, MetaStructureEntryDataType.Float, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.scaleZ, 68, MetaStructureEntryDataType.Float, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.parentIndex, 72, MetaStructureEntryDataType.SignedInt, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.lodDist, 76, MetaStructureEntryDataType.Float, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.childLodDist, 80, MetaStructureEntryDataType.Float, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.lodLevel, 84, MetaStructureEntryDataType.IntEnum, 0, 0, (MetaName)1264241711),
                    new MetaStructureEntryInfo_s(MetaName.numChildren, 88, MetaStructureEntryDataType.UnsignedInt, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.priorityLevel, 92, MetaStructureEntryDataType.IntEnum, 0, 0, (MetaName)648413703),
                    new MetaStructureEntryInfo_s(MetaName.ARRAYINFO, 0, MetaStructureEntryDataType.StructurePointer, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.extensions, 96, MetaStructureEntryDataType.Array, 0, 13, 0),
                    new MetaStructureEntryInfo_s(MetaName.ambientOcclusionMultiplier, 112, MetaStructureEntryDataType.SignedInt, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.artificialAmbientOcclusion, 116, MetaStructureEntryDataType.SignedInt, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.tintValue, 120, MetaStructureEntryDataType.UnsignedInt, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.groupId, 128, MetaStructureEntryDataType.UnsignedInt, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.floorId, 132, MetaStructureEntryDataType.UnsignedInt, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.ARRAYINFO, 0, MetaStructureEntryDataType.Hash, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.defaultEntitySets, 136, MetaStructureEntryDataType.Array, 0, 20, 0),
                    new MetaStructureEntryInfo_s(MetaName.numExitPortals, 152, MetaStructureEntryDataType.UnsignedInt, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.MLOInstflags, 156, MetaStructureEntryDataType.UnsignedInt, 0, 0, 0)
                    );
                case MetaName.CMloPortalDef:
                    return new MetaStructureInfo(MetaName.CMloPortalDef, 1110221513, 768, 64,
                    new MetaStructureEntryInfo_s(MetaName.roomFrom, 8, MetaStructureEntryDataType.UnsignedInt, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.roomTo, 12, MetaStructureEntryDataType.UnsignedInt, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.flags, 16, MetaStructureEntryDataType.UnsignedInt, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.mirrorPriority, 20, MetaStructureEntryDataType.UnsignedInt, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.opacity, 24, MetaStructureEntryDataType.UnsignedInt, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.audioOcclusion, 28, MetaStructureEntryDataType.UnsignedInt, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.ARRAYINFO, 0, MetaStructureEntryDataType.Float_XYZ, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.corners, 32, MetaStructureEntryDataType.Array, 0, 6, 0),
                    new MetaStructureEntryInfo_s(MetaName.ARRAYINFO, 0, MetaStructureEntryDataType.UnsignedInt, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.attachedObjects, 48, MetaStructureEntryDataType.Array, 0, 8, 0)
                    );
                case MetaName.CMapTypes:
                    return new MetaStructureInfo(MetaName.CMapTypes, 2608875220, 768, 80,
                    new MetaStructureEntryInfo_s(MetaName.ARRAYINFO, 0, MetaStructureEntryDataType.StructurePointer, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.extensions, 8, MetaStructureEntryDataType.Array, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.ARRAYINFO, 0, MetaStructureEntryDataType.StructurePointer, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.archetypes, 24, MetaStructureEntryDataType.Array, 0, 2, 0),
                    new MetaStructureEntryInfo_s(MetaName.name, 40, MetaStructureEntryDataType.Hash, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.ARRAYINFO, 0, MetaStructureEntryDataType.Hash, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.dependencies, 48, MetaStructureEntryDataType.Array, 0, 5, 0),
                    new MetaStructureEntryInfo_s(MetaName.ARRAYINFO, 0, MetaStructureEntryDataType.Structure, 0, 0, MetaName.CCompositeEntityType),
                    new MetaStructureEntryInfo_s(MetaName.compositeEntityTypes, 64, MetaStructureEntryDataType.Array, 0, 7, 0)
                    );
                case MetaName.CBaseArchetypeDef:
                    return new MetaStructureInfo(MetaName.CBaseArchetypeDef, 2411387556, 1024, 144,
                    new MetaStructureEntryInfo_s(MetaName.lodDist, 8, MetaStructureEntryDataType.Float, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.flags, 12, MetaStructureEntryDataType.UnsignedInt, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.specialAttribute, 16, MetaStructureEntryDataType.UnsignedInt, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.bbMin, 32, MetaStructureEntryDataType.Float_XYZ, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.bbMax, 48, MetaStructureEntryDataType.Float_XYZ, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.bsCentre, 64, MetaStructureEntryDataType.Float_XYZ, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.bsRadius, 80, MetaStructureEntryDataType.Float, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.hdTextureDist, 84, MetaStructureEntryDataType.Float, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.name, 88, MetaStructureEntryDataType.Hash, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.textureDictionary, 92, MetaStructureEntryDataType.Hash, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.clipDictionary, 96, MetaStructureEntryDataType.Hash, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.drawableDictionary, 100, MetaStructureEntryDataType.Hash, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.physicsDictionary, 104, MetaStructureEntryDataType.Hash, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.assetType, 108, MetaStructureEntryDataType.IntEnum, 0, 0, (MetaName)1991964615),
                    new MetaStructureEntryInfo_s(MetaName.assetName, 112, MetaStructureEntryDataType.Hash, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.ARRAYINFO, 0, MetaStructureEntryDataType.StructurePointer, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.extensions, 120, MetaStructureEntryDataType.Array, 0, 15, 0)
                    );
                case MetaName.CExtensionDefParticleEffect:
                    return new MetaStructureInfo(MetaName.CExtensionDefParticleEffect, 466596385, 1024, 96,
                    new MetaStructureEntryInfo_s(MetaName.name, 8, MetaStructureEntryDataType.Hash, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.offsetPosition, 16, MetaStructureEntryDataType.Float_XYZ, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.offsetRotation, 32, MetaStructureEntryDataType.Float_XYZW, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.fxName, 48, MetaStructureEntryDataType.CharPointer, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.fxType, 64, MetaStructureEntryDataType.SignedInt, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.boneTag, 68, MetaStructureEntryDataType.SignedInt, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.Scale_, 72, MetaStructureEntryDataType.Float, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.probability, 76, MetaStructureEntryDataType.SignedInt, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.flags, 80, MetaStructureEntryDataType.SignedInt, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.color, 84, MetaStructureEntryDataType.UnsignedInt, 0, 0, 0)
                    );
                case MetaName.CMloArchetypeDef:
                    return new MetaStructureInfo(MetaName.CMloArchetypeDef, 937664754, 1024, 240,
                    new MetaStructureEntryInfo_s(MetaName.lodDist, 8, MetaStructureEntryDataType.Float, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.flags, 12, MetaStructureEntryDataType.UnsignedInt, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.specialAttribute, 16, MetaStructureEntryDataType.UnsignedInt, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.bbMin, 32, MetaStructureEntryDataType.Float_XYZ, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.bbMax, 48, MetaStructureEntryDataType.Float_XYZ, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.bsCentre, 64, MetaStructureEntryDataType.Float_XYZ, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.bsRadius, 80, MetaStructureEntryDataType.Float, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.hdTextureDist, 84, MetaStructureEntryDataType.Float, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.name, 88, MetaStructureEntryDataType.Hash, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.textureDictionary, 92, MetaStructureEntryDataType.Hash, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.clipDictionary, 96, MetaStructureEntryDataType.Hash, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.drawableDictionary, 100, MetaStructureEntryDataType.Hash, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.physicsDictionary, 104, MetaStructureEntryDataType.Hash, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.assetType, 108, MetaStructureEntryDataType.IntEnum, 0, 0, (MetaName)1991964615),
                    new MetaStructureEntryInfo_s(MetaName.assetName, 112, MetaStructureEntryDataType.Hash, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.ARRAYINFO, 0, MetaStructureEntryDataType.StructurePointer, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.extensions, 120, MetaStructureEntryDataType.Array, 0, 15, 0),
                    new MetaStructureEntryInfo_s(MetaName.mloFlags, 144, MetaStructureEntryDataType.UnsignedInt, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.ARRAYINFO, 0, MetaStructureEntryDataType.StructurePointer, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.entities, 152, MetaStructureEntryDataType.Array, 0, 18, 0),
                    new MetaStructureEntryInfo_s(MetaName.ARRAYINFO, 0, MetaStructureEntryDataType.Structure, 0, 0, MetaName.CMloRoomDef),
                    new MetaStructureEntryInfo_s(MetaName.rooms, 168, MetaStructureEntryDataType.Array, 0, 20, 0),
                    new MetaStructureEntryInfo_s(MetaName.ARRAYINFO, 0, MetaStructureEntryDataType.Structure, 0, 0, MetaName.CMloPortalDef),
                    new MetaStructureEntryInfo_s(MetaName.portals, 184, MetaStructureEntryDataType.Array, 0, 22, 0),
                    new MetaStructureEntryInfo_s(MetaName.ARRAYINFO, 0, MetaStructureEntryDataType.Structure, 0, 0, MetaName.CMloEntitySet),
                    new MetaStructureEntryInfo_s(MetaName.entitySets, 200, MetaStructureEntryDataType.Array, 0, 24, 0),
                    new MetaStructureEntryInfo_s(MetaName.ARRAYINFO, 0, MetaStructureEntryDataType.Structure, 0, 0, MetaName.CMloTimeCycleModifier),
                    new MetaStructureEntryInfo_s(MetaName.timeCycleModifiers, 216, MetaStructureEntryDataType.Array, 0, 26, 0)
                    );
                case MetaName.CMloRoomDef:
                    return new MetaStructureInfo(MetaName.CMloRoomDef, 3885428245, 1024, 112,
                    new MetaStructureEntryInfo_s(MetaName.name, 8, MetaStructureEntryDataType.CharPointer, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.bbMin, 32, MetaStructureEntryDataType.Float_XYZ, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.bbMax, 48, MetaStructureEntryDataType.Float_XYZ, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.blend, 64, MetaStructureEntryDataType.Float, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.timecycleName, 68, MetaStructureEntryDataType.Hash, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.secondaryTimecycleName, 72, MetaStructureEntryDataType.Hash, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.flags, 76, MetaStructureEntryDataType.UnsignedInt, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.portalCount, 80, MetaStructureEntryDataType.UnsignedInt, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.floorId, 84, MetaStructureEntryDataType.SignedInt, 0, 0, 0),
                    new MetaStructureEntryInfo_s((MetaName)552849982, 88, MetaStructureEntryDataType.SignedInt, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.ARRAYINFO, 0, MetaStructureEntryDataType.UnsignedInt, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.attachedObjects, 96, MetaStructureEntryDataType.Array, 0, 10, 0)
                    );
                case MetaName.CTimeArchetypeDef:
                    return new MetaStructureInfo(MetaName.CTimeArchetypeDef, 2520619910, 1024, 160,
                    new MetaStructureEntryInfo_s(MetaName.lodDist, 8, MetaStructureEntryDataType.Float, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.flags, 12, MetaStructureEntryDataType.UnsignedInt, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.specialAttribute, 16, MetaStructureEntryDataType.UnsignedInt, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.bbMin, 32, MetaStructureEntryDataType.Float_XYZ, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.bbMax, 48, MetaStructureEntryDataType.Float_XYZ, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.bsCentre, 64, MetaStructureEntryDataType.Float_XYZ, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.bsRadius, 80, MetaStructureEntryDataType.Float, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.hdTextureDist, 84, MetaStructureEntryDataType.Float, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.name, 88, MetaStructureEntryDataType.Hash, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.textureDictionary, 92, MetaStructureEntryDataType.Hash, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.clipDictionary, 96, MetaStructureEntryDataType.Hash, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.drawableDictionary, 100, MetaStructureEntryDataType.Hash, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.physicsDictionary, 104, MetaStructureEntryDataType.Hash, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.assetType, 108, MetaStructureEntryDataType.IntEnum, 0, 0, (MetaName)1991964615),
                    new MetaStructureEntryInfo_s(MetaName.assetName, 112, MetaStructureEntryDataType.Hash, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.ARRAYINFO, 0, MetaStructureEntryDataType.StructurePointer, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.extensions, 120, MetaStructureEntryDataType.Array, 0, 15, 0),
                    new MetaStructureEntryInfo_s(MetaName.timeFlags, 144, MetaStructureEntryDataType.UnsignedInt, 0, 0, 0)
                    );

                case MetaName.CTimeCycleModifier:
                    return new MetaStructureInfo(MetaName.CTimeCycleModifier, 2683420777, 1024, 64,
                    new MetaStructureEntryInfo_s(MetaName.name, 8, MetaStructureEntryDataType.Hash, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.minExtents, 16, MetaStructureEntryDataType.Float_XYZ, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.maxExtents, 32, MetaStructureEntryDataType.Float_XYZ, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.percentage, 48, MetaStructureEntryDataType.Float, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.range, 52, MetaStructureEntryDataType.Float, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.startHour, 56, MetaStructureEntryDataType.UnsignedInt, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.endHour, 60, MetaStructureEntryDataType.UnsignedInt, 0, 0, 0)
                    );
                case MetaName.CCarGen:
                    return new MetaStructureInfo(MetaName.CCarGen, 2345238261, 1024, 80,
                    new MetaStructureEntryInfo_s(MetaName.position, 16, MetaStructureEntryDataType.Float_XYZ, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.orientX, 32, MetaStructureEntryDataType.Float, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.orientY, 36, MetaStructureEntryDataType.Float, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.perpendicularLength, 40, MetaStructureEntryDataType.Float, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.carModel, 44, MetaStructureEntryDataType.Hash, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.flags, 48, MetaStructureEntryDataType.UnsignedInt, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.bodyColorRemap1, 52, MetaStructureEntryDataType.SignedInt, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.bodyColorRemap2, 56, MetaStructureEntryDataType.SignedInt, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.bodyColorRemap3, 60, MetaStructureEntryDataType.SignedInt, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.bodyColorRemap4, 64, MetaStructureEntryDataType.SignedInt, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.popGroup, 68, MetaStructureEntryDataType.Hash, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.livery, 72, MetaStructureEntryDataType.SignedByte, 0, 0, 0)
                    );
                case MetaName.CExtensionDefLightEffect:
                    return new MetaStructureInfo(MetaName.CExtensionDefLightEffect, 2436199897, 1024, 48,
                    new MetaStructureEntryInfo_s(MetaName.name, 8, MetaStructureEntryDataType.Hash, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.offsetPosition, 16, MetaStructureEntryDataType.Float_XYZ, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.ARRAYINFO, 0, MetaStructureEntryDataType.Structure, 0, 0, MetaName.CLightAttrDef),
                    new MetaStructureEntryInfo_s(MetaName.instances, 32, MetaStructureEntryDataType.Array, 0, 2, 0)
                    );
                case MetaName.CLightAttrDef:
                    return new MetaStructureInfo(MetaName.CLightAttrDef, 2363260268, 768, 160,
                    new MetaStructureEntryInfo_s(MetaName.ARRAYINFO, 0, MetaStructureEntryDataType.Float, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.posn, 8, MetaStructureEntryDataType.ArrayOfBytes, 0, 0, (MetaName)3),
                    new MetaStructureEntryInfo_s(MetaName.ARRAYINFO, 0, MetaStructureEntryDataType.UnsignedByte, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.colour, 20, MetaStructureEntryDataType.ArrayOfBytes, 0, 2, (MetaName)3),
                    new MetaStructureEntryInfo_s(MetaName.flashiness, 23, MetaStructureEntryDataType.UnsignedByte, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.intensity, 24, MetaStructureEntryDataType.Float, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.flags, 28, MetaStructureEntryDataType.UnsignedInt, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.boneTag, 32, MetaStructureEntryDataType.SignedShort, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.lightType, 34, MetaStructureEntryDataType.UnsignedByte, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.groupId, 35, MetaStructureEntryDataType.UnsignedByte, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.timeFlags, 36, MetaStructureEntryDataType.UnsignedInt, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.falloff, 40, MetaStructureEntryDataType.Float, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.falloffExponent, 44, MetaStructureEntryDataType.Float, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.ARRAYINFO, 0, MetaStructureEntryDataType.Float, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.cullingPlane, 48, MetaStructureEntryDataType.ArrayOfBytes, 0, 13, (MetaName)4),
                    new MetaStructureEntryInfo_s(MetaName.shadowBlur, 64, MetaStructureEntryDataType.UnsignedByte, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.padding1, 65, MetaStructureEntryDataType.UnsignedByte, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.padding2, 66, MetaStructureEntryDataType.SignedShort, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.padding3, 68, MetaStructureEntryDataType.UnsignedInt, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.volIntensity, 72, MetaStructureEntryDataType.Float, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.volSizeScale, 76, MetaStructureEntryDataType.Float, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.ARRAYINFO, 0, MetaStructureEntryDataType.UnsignedByte, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.volOuterColour, 80, MetaStructureEntryDataType.ArrayOfBytes, 0, 21, (MetaName)3),
                    new MetaStructureEntryInfo_s(MetaName.lightHash, 83, MetaStructureEntryDataType.UnsignedByte, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.volOuterIntensity, 84, MetaStructureEntryDataType.Float, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.coronaSize, 88, MetaStructureEntryDataType.Float, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.volOuterExponent, 92, MetaStructureEntryDataType.Float, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.lightFadeDistance, 96, MetaStructureEntryDataType.UnsignedByte, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.shadowFadeDistance, 97, MetaStructureEntryDataType.UnsignedByte, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.specularFadeDistance, 98, MetaStructureEntryDataType.UnsignedByte, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.volumetricFadeDistance, 99, MetaStructureEntryDataType.UnsignedByte, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.shadowNearClip, 100, MetaStructureEntryDataType.Float, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.coronaIntensity, 104, MetaStructureEntryDataType.Float, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.coronaZBias, 108, MetaStructureEntryDataType.Float, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.ARRAYINFO, 0, MetaStructureEntryDataType.Float, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.direction, 112, MetaStructureEntryDataType.ArrayOfBytes, 0, 34, (MetaName)3),
                    new MetaStructureEntryInfo_s(MetaName.ARRAYINFO, 0, MetaStructureEntryDataType.Float, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.tangent, 124, MetaStructureEntryDataType.ArrayOfBytes, 0, 36, (MetaName)3),
                    new MetaStructureEntryInfo_s(MetaName.coneInnerAngle, 136, MetaStructureEntryDataType.Float, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.coneOuterAngle, 140, MetaStructureEntryDataType.Float, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.ARRAYINFO, 0, MetaStructureEntryDataType.Float, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.extents, 144, MetaStructureEntryDataType.ArrayOfBytes, 0, 40, (MetaName)3),
                    new MetaStructureEntryInfo_s(MetaName.projectedTextureKey, 156, MetaStructureEntryDataType.UnsignedInt, 0, 0, 0)
                    );
                case MetaName.CExtensionDefSpawnPointOverride:
                    return new MetaStructureInfo(MetaName.CExtensionDefSpawnPointOverride, 2551875873, 1024, 64,
                    new MetaStructureEntryInfo_s(MetaName.name, 8, MetaStructureEntryDataType.Hash, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.offsetPosition, 16, MetaStructureEntryDataType.Float_XYZ, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.ScenarioType, 32, MetaStructureEntryDataType.Hash, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.iTimeStartOverride, 36, MetaStructureEntryDataType.UnsignedByte, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.iTimeEndOverride, 37, MetaStructureEntryDataType.UnsignedByte, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.Group, 40, MetaStructureEntryDataType.Hash, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.ModelSet, 44, MetaStructureEntryDataType.Hash, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.AvailabilityInMpSp, 48, MetaStructureEntryDataType.IntEnum, 0, 0, (MetaName)3573596290),
                    new MetaStructureEntryInfo_s(MetaName.Flags, 52, MetaStructureEntryDataType.IntFlags2, 0, 32, (MetaName)700327466),
                    new MetaStructureEntryInfo_s(MetaName.Radius, 56, MetaStructureEntryDataType.Float, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.TimeTillPedLeaves, 60, MetaStructureEntryDataType.Float, 0, 0, 0)
                    );
                case MetaName.CExtensionDefDoor:
                    return new MetaStructureInfo(MetaName.CExtensionDefDoor, 2671601385, 1024, 48,
                    new MetaStructureEntryInfo_s(MetaName.name, 8, MetaStructureEntryDataType.Hash, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.offsetPosition, 16, MetaStructureEntryDataType.Float_XYZ, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.enableLimitAngle, 32, MetaStructureEntryDataType.Boolean, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.startsLocked, 33, MetaStructureEntryDataType.Boolean, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.canBreak, 34, MetaStructureEntryDataType.Boolean, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.limitAngle, 36, MetaStructureEntryDataType.Float, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.doorTargetRatio, 40, MetaStructureEntryDataType.Float, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.audioHash, 44, MetaStructureEntryDataType.Hash, 0, 0, 0)
                    );
                case MetaName.rage__phVerletClothCustomBounds:
                    return new MetaStructureInfo(MetaName.rage__phVerletClothCustomBounds, 2075461750, 768, 32,
                    new MetaStructureEntryInfo_s(MetaName.name, 8, MetaStructureEntryDataType.Hash, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.ARRAYINFO, 0, MetaStructureEntryDataType.Structure, 0, 0, (MetaName)1701774085/*.SectionUNKNOWN1*/),
                    new MetaStructureEntryInfo_s(MetaName.CollisionData, 16, MetaStructureEntryDataType.Array, 0, 1, 0)
                    );
                case (MetaName)1701774085/*.SectionUNKNOWN1*/:
                    return new MetaStructureInfo((MetaName)1701774085/*.SectionUNKNOWN1*/, 2859775340, 1024, 96,
                    new MetaStructureEntryInfo_s(MetaName.OwnerName, 0, MetaStructureEntryDataType.CharPointer, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.Rotation, 16, MetaStructureEntryDataType.Float_XYZW, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.Position, 32, MetaStructureEntryDataType.Float_XYZ, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.Normal, 48, MetaStructureEntryDataType.Float_XYZ, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.CapsuleRadius, 64, MetaStructureEntryDataType.Float, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.CapsuleLen, 68, MetaStructureEntryDataType.Float, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.CapsuleHalfHeight, 72, MetaStructureEntryDataType.Float, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.CapsuleHalfWidth, 76, MetaStructureEntryDataType.Float, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.Flags, 80, MetaStructureEntryDataType.IntFlags2, 0, 32, (MetaName)3044470860)
                    );



                /* SCENARIO types */
                case MetaName.CScenarioPointContainer:
                    return new MetaStructureInfo(MetaName.CScenarioPointContainer, 2489654897, 768, 48,
                    new MetaStructureEntryInfo_s(MetaName.ARRAYINFO, 0, MetaStructureEntryDataType.Structure, 0, 0, MetaName.CExtensionDefSpawnPoint),
                    new MetaStructureEntryInfo_s(MetaName.LoadSavePoints, 0, MetaStructureEntryDataType.Array, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.ARRAYINFO, 0, MetaStructureEntryDataType.Structure, 0, 0, MetaName.CScenarioPoint),
                    new MetaStructureEntryInfo_s(MetaName.MyPoints, 16, MetaStructureEntryDataType.Array, 0, 2, 0)
                    );
                case (MetaName)4023740759:
                    return new MetaStructureInfo((MetaName)4023740759, 88255871, 768, 88,
                    new MetaStructureEntryInfo_s(MetaName.ARRAYINFO, 0, MetaStructureEntryDataType.Structure, 0, 0, MetaName.CScenarioChainingNode),
                    new MetaStructureEntryInfo_s(MetaName.Nodes, 0, MetaStructureEntryDataType.Array, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.ARRAYINFO, 0, MetaStructureEntryDataType.Structure, 0, 0, MetaName.CScenarioChainingEdge),
                    new MetaStructureEntryInfo_s(MetaName.Edges, 16, MetaStructureEntryDataType.Array, 0, 2, 0),
                    new MetaStructureEntryInfo_s(MetaName.ARRAYINFO, 0, MetaStructureEntryDataType.Structure, 0, 0, MetaName.CScenarioChain),
                    new MetaStructureEntryInfo_s(MetaName.Chains, 32, MetaStructureEntryDataType.Array, 0, 4, 0)
                    );
                case MetaName.rage__spdGrid2D:
                    return new MetaStructureInfo(MetaName.rage__spdGrid2D, 894636096, 768, 64,
                    new MetaStructureEntryInfo_s((MetaName)860552138, 12, MetaStructureEntryDataType.SignedInt, 0, 0, 0),
                    new MetaStructureEntryInfo_s((MetaName)3824598937, 16, MetaStructureEntryDataType.SignedInt, 0, 0, 0),
                    new MetaStructureEntryInfo_s((MetaName)496029782, 20, MetaStructureEntryDataType.SignedInt, 0, 0, 0),
                    new MetaStructureEntryInfo_s((MetaName)3374647798, 24, MetaStructureEntryDataType.SignedInt, 0, 0, 0),
                    new MetaStructureEntryInfo_s((MetaName)2690909759, 44, MetaStructureEntryDataType.Float, 0, 0, 0),
                    new MetaStructureEntryInfo_s((MetaName)3691675019, 48, MetaStructureEntryDataType.Float, 0, 0, 0)
                    );
                case MetaName.CScenarioPointLookUps:
                    return new MetaStructureInfo(MetaName.CScenarioPointLookUps, 2669361587, 768, 96,
                    new MetaStructureEntryInfo_s(MetaName.ARRAYINFO, 0, MetaStructureEntryDataType.Hash, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.TypeNames, 0, MetaStructureEntryDataType.Array, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.ARRAYINFO, 0, MetaStructureEntryDataType.Hash, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.PedModelSetNames, 16, MetaStructureEntryDataType.Array, 0, 2, 0),
                    new MetaStructureEntryInfo_s(MetaName.ARRAYINFO, 0, MetaStructureEntryDataType.Hash, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.VehicleModelSetNames, 32, MetaStructureEntryDataType.Array, 0, 4, 0),
                    new MetaStructureEntryInfo_s(MetaName.ARRAYINFO, 0, MetaStructureEntryDataType.Hash, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.GroupNames, 48, MetaStructureEntryDataType.Array, 0, 6, 0),
                    new MetaStructureEntryInfo_s(MetaName.ARRAYINFO, 0, MetaStructureEntryDataType.Hash, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.InteriorNames, 64, MetaStructureEntryDataType.Array, 0, 8, 0),
                    new MetaStructureEntryInfo_s(MetaName.ARRAYINFO, 0, MetaStructureEntryDataType.Hash, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.RequiredIMapNames, 80, MetaStructureEntryDataType.Array, 0, 10, 0)
                    );
                case MetaName.CScenarioPointRegion:
                    return new MetaStructureInfo(MetaName.CScenarioPointRegion, 3501351821, 768, 376,
                    new MetaStructureEntryInfo_s(MetaName.VersionNumber, 0, MetaStructureEntryDataType.SignedInt, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.Points, 8, MetaStructureEntryDataType.Structure, 0, 0, MetaName.CScenarioPointContainer),
                    new MetaStructureEntryInfo_s(MetaName.ARRAYINFO, 0, MetaStructureEntryDataType.Structure, 0, 0, MetaName.CScenarioEntityOverride),
                    new MetaStructureEntryInfo_s(MetaName.EntityOverrides, 72, MetaStructureEntryDataType.Array, 0, 2, 0),
                    new MetaStructureEntryInfo_s((MetaName)3696045377, 96, MetaStructureEntryDataType.Structure, 0, 0, (MetaName)4023740759),
                    new MetaStructureEntryInfo_s(MetaName.AccelGrid, 184, MetaStructureEntryDataType.Structure, 0, 0, MetaName.rage__spdGrid2D),
                    new MetaStructureEntryInfo_s(MetaName.ARRAYINFO, 0, MetaStructureEntryDataType.UnsignedShort, 0, 0, 0),
                    new MetaStructureEntryInfo_s((MetaName)3844724227, 248, MetaStructureEntryDataType.Array, 0, 6, 0),
                    new MetaStructureEntryInfo_s(MetaName.ARRAYINFO, 0, MetaStructureEntryDataType.Structure, 0, 0, MetaName.CScenarioPointCluster),
                    new MetaStructureEntryInfo_s(MetaName.Clusters, 264, MetaStructureEntryDataType.Array, 0, 8, 0),
                    new MetaStructureEntryInfo_s(MetaName.LookUps, 280, MetaStructureEntryDataType.Structure, 0, 0, MetaName.CScenarioPointLookUps)
                    );
                case MetaName.CScenarioPoint:
                    return new MetaStructureInfo(MetaName.CScenarioPoint, 402442150, 1024, 64,
                    new MetaStructureEntryInfo_s(MetaName.iType, 21, MetaStructureEntryDataType.UnsignedByte, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.ModelSetId, 22, MetaStructureEntryDataType.UnsignedByte, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.iInterior, 23, MetaStructureEntryDataType.UnsignedByte, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.iRequiredIMapId, 24, MetaStructureEntryDataType.UnsignedByte, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.iProbability, 25, MetaStructureEntryDataType.UnsignedByte, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.uAvailableInMpSp, 26, MetaStructureEntryDataType.UnsignedByte, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.iTimeStartOverride, 27, MetaStructureEntryDataType.UnsignedByte, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.iTimeEndOverride, 28, MetaStructureEntryDataType.UnsignedByte, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.iRadius, 29, MetaStructureEntryDataType.UnsignedByte, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.iTimeTillPedLeaves, 30, MetaStructureEntryDataType.UnsignedByte, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.iScenarioGroup, 32, MetaStructureEntryDataType.UnsignedShort, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.Flags, 36, MetaStructureEntryDataType.IntFlags2, 0, 32, (MetaName)700327466),
                    new MetaStructureEntryInfo_s(MetaName.vPositionAndDirection, 48, MetaStructureEntryDataType.Float_XYZW, 0, 0, 0)
                    );
                case MetaName.CScenarioEntityOverride:
                    return new MetaStructureInfo(MetaName.CScenarioEntityOverride, 1271200492, 1024, 80,
                    new MetaStructureEntryInfo_s(MetaName.EntityPosition, 0, MetaStructureEntryDataType.Float_XYZ, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.EntityType, 16, MetaStructureEntryDataType.Hash, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.ARRAYINFO, 0, MetaStructureEntryDataType.Structure, 0, 0, MetaName.CExtensionDefSpawnPoint),
                    new MetaStructureEntryInfo_s(MetaName.ScenarioPoints, 24, MetaStructureEntryDataType.Array, 0, 2, 0),
                    new MetaStructureEntryInfo_s((MetaName)538733109, 64, MetaStructureEntryDataType.Boolean, 0, 0, 0),
                    new MetaStructureEntryInfo_s((MetaName)1035513142, 65, MetaStructureEntryDataType.Boolean, 0, 0, 0)
                    );
                case MetaName.CExtensionDefSpawnPoint:
                    return new MetaStructureInfo(MetaName.CExtensionDefSpawnPoint, 3077340721, 1024, 96,
                    new MetaStructureEntryInfo_s(MetaName.name, 8, MetaStructureEntryDataType.Hash, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.offsetPosition, 16, MetaStructureEntryDataType.Float_XYZ, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.offsetRotation, 32, MetaStructureEntryDataType.Float_XYZW, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.spawnType, 48, MetaStructureEntryDataType.Hash, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.pedType, 52, MetaStructureEntryDataType.Hash, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.group, 56, MetaStructureEntryDataType.Hash, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.interior, 60, MetaStructureEntryDataType.Hash, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.requiredImap, 64, MetaStructureEntryDataType.Hash, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.availableInMpSp, 68, MetaStructureEntryDataType.IntEnum, 0, 0, (MetaName)3573596290),
                    new MetaStructureEntryInfo_s(MetaName.probability, 72, MetaStructureEntryDataType.Float, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.timeTillPedLeaves, 76, MetaStructureEntryDataType.Float, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.radius, 80, MetaStructureEntryDataType.Float, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.start, 84, MetaStructureEntryDataType.UnsignedByte, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.end, 85, MetaStructureEntryDataType.UnsignedByte, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.flags, 88, MetaStructureEntryDataType.IntFlags2, 0, 32, (MetaName)700327466),
                    new MetaStructureEntryInfo_s(MetaName.highPri, 92, MetaStructureEntryDataType.Boolean, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.extendedRange, 93, MetaStructureEntryDataType.Boolean, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.shortRange, 94, MetaStructureEntryDataType.Boolean, 0, 0, 0)
                    );
                case MetaName.CScenarioChainingNode:
                    return new MetaStructureInfo(MetaName.CScenarioChainingNode, 1811784424, 1024, 32,
                    new MetaStructureEntryInfo_s(MetaName.Position, 0, MetaStructureEntryDataType.Float_XYZ, 0, 0, 0),
                    new MetaStructureEntryInfo_s((MetaName)2602393771, 16, MetaStructureEntryDataType.Hash, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.ScenarioType, 20, MetaStructureEntryDataType.Hash, 0, 0, 0),
                    new MetaStructureEntryInfo_s((MetaName)407126079, 24, MetaStructureEntryDataType.Boolean, 0, 0, 0),
                    new MetaStructureEntryInfo_s((MetaName)1308720135, 25, MetaStructureEntryDataType.Boolean, 0, 0, 0)
                    );
                case MetaName.CScenarioChainingEdge:
                    return new MetaStructureInfo(MetaName.CScenarioChainingEdge, 2004985940, 256, 8,
                    new MetaStructureEntryInfo_s(MetaName.NodeIndexFrom, 0, MetaStructureEntryDataType.UnsignedShort, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.NodeIndexTo, 2, MetaStructureEntryDataType.UnsignedShort, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.Action, 4, MetaStructureEntryDataType.ByteEnum, 0, 0, (MetaName)3609807418),
                    new MetaStructureEntryInfo_s(MetaName.NavMode, 5, MetaStructureEntryDataType.ByteEnum, 0, 0, (MetaName)3971773454),
                    new MetaStructureEntryInfo_s(MetaName.NavSpeed, 6, MetaStructureEntryDataType.ByteEnum, 0, 0, (MetaName)941086046)
                    );
                case MetaName.CScenarioChain:
                    return new MetaStructureInfo(MetaName.CScenarioChain, 2751910366, 768, 40,
                    new MetaStructureEntryInfo_s((MetaName)1156691834, 0, MetaStructureEntryDataType.UnsignedByte, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.ARRAYINFO, 0, MetaStructureEntryDataType.UnsignedShort, 0, 0, 0),
                    new MetaStructureEntryInfo_s(MetaName.EdgeIds, 8, MetaStructureEntryDataType.Array, 0, 1, 0)
                    );
                case MetaName.rage__spdSphere:
                    return new MetaStructureInfo(MetaName.rage__spdSphere, 1189037266, 1024, 16,
                    new MetaStructureEntryInfo_s(MetaName.centerAndRadius, 0, MetaStructureEntryDataType.Float_XYZW, 0, 0, 0)
                    );
                case MetaName.CScenarioPointCluster:
                    return new MetaStructureInfo(MetaName.CScenarioPointCluster, 3622480419, 1024, 80,
                    new MetaStructureEntryInfo_s(MetaName.Points, 0, MetaStructureEntryDataType.Structure, 0, 0, MetaName.CScenarioPointContainer),
                    new MetaStructureEntryInfo_s(MetaName.ClusterSphere, 48, MetaStructureEntryDataType.Structure, 0, 0, MetaName.rage__spdSphere),
                    new MetaStructureEntryInfo_s((MetaName)1095875445, 64, MetaStructureEntryDataType.Float, 0, 0, 0),
                    new MetaStructureEntryInfo_s((MetaName)3129415068, 68, MetaStructureEntryDataType.Boolean, 0, 0, 0)
                    );



                default:
                    return null;
            }
        }
        public static MetaEnumInfo GetEnumInfo(MetaName name)
        {
            //to generate enuminfos
            switch (name)
            {
                case (MetaName)1264241711:
                    return new MetaEnumInfo((MetaName)1264241711, 1856311430,
                    new MetaEnumEntryInfo_s(MetaName.LODTYPES_DEPTH_HD, 0),
                    new MetaEnumEntryInfo_s(MetaName.LODTYPES_DEPTH_LOD, 1),
                    new MetaEnumEntryInfo_s(MetaName.LODTYPES_DEPTH_SLOD1, 2),
                    new MetaEnumEntryInfo_s(MetaName.LODTYPES_DEPTH_SLOD2, 3),
                    new MetaEnumEntryInfo_s(MetaName.LODTYPES_DEPTH_SLOD3, 4),
                    new MetaEnumEntryInfo_s(MetaName.LODTYPES_DEPTH_ORPHANHD, 5),
                    new MetaEnumEntryInfo_s(MetaName.LODTYPES_DEPTH_SLOD4, 6)
                    );
                case (MetaName)648413703:
                    return new MetaEnumInfo((MetaName)648413703, 2200357711,
                    new MetaEnumEntryInfo_s(MetaName.PRI_REQUIRED, 0),
                    new MetaEnumEntryInfo_s(MetaName.PRI_OPTIONAL_HIGH, 1),
                    new MetaEnumEntryInfo_s(MetaName.PRI_OPTIONAL_MEDIUM, 2),
                    new MetaEnumEntryInfo_s(MetaName.PRI_OPTIONAL_LOW, 3)
                    );
                case (MetaName)3573596290:
                    return new MetaEnumInfo((MetaName)3573596290, 671739257,
                    new MetaEnumEntryInfo_s(MetaName.kBoth, 0),
                    new MetaEnumEntryInfo_s(MetaName.kOnlySp, 1),
                    new MetaEnumEntryInfo_s(MetaName.kOnlyMp, 2)
                    );
                case (MetaName)700327466:
                    return new MetaEnumInfo((MetaName)700327466, 2814596095,
                    new MetaEnumEntryInfo_s(MetaName.IgnoreMaxInRange, 0),
                    new MetaEnumEntryInfo_s(MetaName.NoSpawn, 1),
                    new MetaEnumEntryInfo_s(MetaName.StationaryReactions, 2),
                    new MetaEnumEntryInfo_s((MetaName)3257836369, 3),
                    new MetaEnumEntryInfo_s((MetaName)2165609255, 4),
                    new MetaEnumEntryInfo_s(MetaName.ActivateVehicleSiren, 5),
                    new MetaEnumEntryInfo_s(MetaName.AggressiveVehicleDriving, 6),
                    new MetaEnumEntryInfo_s((MetaName)2004780781, 7),
                    new MetaEnumEntryInfo_s((MetaName)536864854, 8),
                    new MetaEnumEntryInfo_s((MetaName)3441065168, 9),
                    new MetaEnumEntryInfo_s(MetaName.AerialVehiclePoint, 10),
                    new MetaEnumEntryInfo_s(MetaName.TerritorialScenario, 11),
                    new MetaEnumEntryInfo_s((MetaName)3690227693, 12),
                    new MetaEnumEntryInfo_s((MetaName)1601179199, 13),
                    new MetaEnumEntryInfo_s((MetaName)2583152330, 14),
                    new MetaEnumEntryInfo_s((MetaName)3490317520, 15),
                    new MetaEnumEntryInfo_s(MetaName.InWater, 16),
                    new MetaEnumEntryInfo_s((MetaName)1269249358, 17),
                    new MetaEnumEntryInfo_s(MetaName.OpenDoor, 18),
                    new MetaEnumEntryInfo_s(MetaName.PreciseUseTime, 19),
                    new MetaEnumEntryInfo_s((MetaName)2247631388, 20),
                    new MetaEnumEntryInfo_s((MetaName)4100708934, 21),
                    new MetaEnumEntryInfo_s(MetaName.ExtendedRange, 22),
                    new MetaEnumEntryInfo_s(MetaName.ShortRange, 23),
                    new MetaEnumEntryInfo_s(MetaName.HighPriority, 24),
                    new MetaEnumEntryInfo_s(MetaName.IgnoreLoitering, 25),
                    new MetaEnumEntryInfo_s(MetaName.UseSearchlight, 26),
                    new MetaEnumEntryInfo_s(MetaName.ResetNoCollisionOnCleanUp, 27),
                    new MetaEnumEntryInfo_s((MetaName)3304563391, 28),
                    new MetaEnumEntryInfo_s((MetaName)1111379709, 29),
                    new MetaEnumEntryInfo_s(MetaName.IgnoreWeatherRestrictions, 30)
                    );
                case (MetaName)3044470860:
                    return new MetaEnumInfo((MetaName)3044470860, 1585854303,
                    new MetaEnumEntryInfo_s((MetaName)997866013, 0)
                    );


                case (MetaName)3609807418:
                    return new MetaEnumInfo((MetaName)3609807418, 3326075799,
                    new MetaEnumEntryInfo_s(MetaName.Move, 0),
                    new MetaEnumEntryInfo_s((MetaName)7865678, 1),
                    new MetaEnumEntryInfo_s(MetaName.MoveFollowMaster, 2)
                    );
                case (MetaName)3971773454:
                    return new MetaEnumInfo((MetaName)3971773454, 3016128742,
                    new MetaEnumEntryInfo_s(MetaName.Direct, 0),
                    new MetaEnumEntryInfo_s(MetaName.NavMesh, 1),
                    new MetaEnumEntryInfo_s(MetaName.Roads, 2)
                    );
                case (MetaName)941086046:
                    return new MetaEnumInfo((MetaName)941086046, 1112851290,
                    new MetaEnumEntryInfo_s((MetaName)3279574318, 0),
                    new MetaEnumEntryInfo_s((MetaName)2212923970, 1),
                    new MetaEnumEntryInfo_s((MetaName)4022799658, 2),
                    new MetaEnumEntryInfo_s((MetaName)1425672334, 3),
                    new MetaEnumEntryInfo_s((MetaName)957720931, 4),
                    new MetaEnumEntryInfo_s((MetaName)3795195414, 5),
                    new MetaEnumEntryInfo_s((MetaName)2834622009, 6),
                    new MetaEnumEntryInfo_s((MetaName)1876554076, 7),
                    new MetaEnumEntryInfo_s((MetaName)698543797, 8),
                    new MetaEnumEntryInfo_s((MetaName)1544199634, 9),
                    new MetaEnumEntryInfo_s((MetaName)2725613303, 10),
                    new MetaEnumEntryInfo_s((MetaName)4033265820, 11),
                    new MetaEnumEntryInfo_s((MetaName)3054809929, 12),
                    new MetaEnumEntryInfo_s((MetaName)3911005380, 13),
                    new MetaEnumEntryInfo_s((MetaName)3717649022, 14),
                    new MetaEnumEntryInfo_s((MetaName)3356026130, 15)
                    );
                case (MetaName)1991964615:
                    return new MetaEnumInfo((MetaName)1991964615, 1866031916,
                    new MetaEnumEntryInfo_s(MetaName.ASSET_TYPE_UNINITIALIZED, 0),
                    new MetaEnumEntryInfo_s(MetaName.ASSET_TYPE_FRAGMENT, 1),
                    new MetaEnumEntryInfo_s(MetaName.ASSET_TYPE_DRAWABLE, 2),
                    new MetaEnumEntryInfo_s(MetaName.ASSET_TYPE_DRAWABLEDICTIONARY, 3),
                    new MetaEnumEntryInfo_s(MetaName.ASSET_TYPE_ASSETLESS, 4)
                    );


                default:
                    return null;
            }
        }




        private static string GetSafeName(MetaName namehash, uint key)
        {
            string name = namehash.ToString();
            if (string.IsNullOrEmpty(name))
            {
                name = "Unk_" + key;
            }
            if (!char.IsLetter(name[0]))
            {
                name = "Unk_" + name;
            }
            return name;
        }





        public static byte[] ConvertToBytes<T>(T item) where T : struct
        {
            int size = Marshal.SizeOf(typeof(T));
            int offset = 0;
            byte[] arr = new byte[size];
            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(item, ptr, true);
            Marshal.Copy(ptr, arr, 0, size);
            Marshal.FreeHGlobal(ptr);
            offset += size;
            return arr;
        }
        public static byte[] ConvertArrayToBytes<T>(params T[] items) where T : struct
        {
            if (items == null) return null;
            int size = Marshal.SizeOf(typeof(T));
            int sizetot = size * items.Length;
            byte[] arrout = new byte[sizetot];
            int offset = 0;
            for (int i = 0; i < items.Length; i++)
            {
                byte[] arr = new byte[size];
                IntPtr ptr = Marshal.AllocHGlobal(size);
                Marshal.StructureToPtr(items[i], ptr, true);
                Marshal.Copy(ptr, arr, 0, size);
                Marshal.FreeHGlobal(ptr);
                Buffer.BlockCopy(arr, 0, arrout, offset, size);
                offset += size;
            }
            return arrout;
        }


        public static T ConvertData<T>(byte[] data) where T : struct
        {
            GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
            var h = handle.AddrOfPinnedObject();
            var r = Marshal.PtrToStructure<T>(h);
            handle.Free();
            return r;
        }
        public static T ConvertData<T>(byte[] data, int offset) where T : struct
        {
            GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
            var h = handle.AddrOfPinnedObject();
            var r = Marshal.PtrToStructure<T>(h + offset);
            handle.Free();
            return r;
        }
        public static T[] ConvertDataArray<T>(byte[] data, int offset, int count) where T : struct
        {
            T[] items = new T[count];
            int itemsize = Marshal.SizeOf(typeof(T));
            for (int i = 0; i < count; i++)
            {
                int off = offset + i * itemsize;
                items[i] = ConvertData<T>(data, off);
            }
            return items;
        }
        public static T[] ConvertDataArray<T>(Meta meta, MetaName name, Array_StructurePointer array) where T : struct
        {
            //return ConvertDataArray<T>(meta, name, array.Pointer, array.Count1);
            uint count = array.Count1;
            if (count == 0) return null;
            MetaPOINTER[] ptrs = GetPointerArray(meta, array);
            if (ptrs == null) return null;
            if (ptrs.Length < count)
            { return null; }

            T[] items = new T[count];
            int itemsize = Marshal.SizeOf(typeof(T));
            int itemsleft = (int)count; //large arrays get split into chunks...

            //MetaName blocktype = 0;
            for (int i = 0; i < count; i++)
            {
                var ptr = ptrs[i];
                var offset = ptr.Offset;
                var block = meta.GetBlock(ptr.BlockID);
                if (block == null) continue;

                //if (blocktype == 0)
                //{ blocktype = block.StructureNameHash; }
                //else if (block.StructureNameHash != blocktype)
                //{ } //not all the same type..!

                if (block.StructureNameHash != name)
                { return null; } //type mismatch - don't return anything...
                if ((offset < 0) || (block.Data == null) || (offset >= block.Data.Length))
                { continue; }
                items[i] = ConvertData<T>(block.Data, offset);
            }

            return items;
        }
        public static T[] ConvertDataArray<T>(Meta meta, MetaName name, Array_Structure array) where T : struct
        {
            return ConvertDataArray<T>(meta, name, array.Pointer, array.Count1);
        }
        public static T[] ConvertDataArray<T>(Meta meta, MetaName name, uint pointer, uint count) where T : struct
        {
            if (count == 0) return null;

            T[] items = new T[count];
            int itemsize = Marshal.SizeOf(typeof(T));
            int itemsleft = (int)count; //large arrays get split into chunks...

            uint ptrindex = (pointer & 0xFFF) - 1;
            uint ptroffset = ((pointer >> 12) & 0xFFFFF);
            var ptrblock = (ptrindex < meta.DataBlocks.Count) ? meta.DataBlocks[(int)ptrindex] : null;
            if ((ptrblock == null) || (ptrblock.Data == null) || (ptrblock.StructureNameHash != name))
            { return null; } //no block or wrong block? shouldn't happen!

            int byteoffset = (int)ptroffset;// (ptroffset * 16 + ptrunkval);
            int itemoffset = byteoffset / itemsize;

            int curi = 0;
            while (itemsleft > 0)
            {
                int blockcount = ptrblock.DataLength / itemsize;
                int itemcount = blockcount - itemoffset;
                if (itemcount > itemsleft)
                { itemcount = itemsleft; } //don't try to read too many items..
                for (int i = 0; i < itemcount; i++)
                {
                    int offset = (itemoffset + i) * itemsize;
                    int index = curi + i;
                    items[index] = ConvertData<T>(ptrblock.Data, offset);
                }
                itemoffset = 0; //start at beginning of next block..
                curi += itemcount;
                itemsleft -= itemcount;
                if (itemsleft <= 0)
                { return items; }//all done!
                ptrindex++;
                ptrblock = (ptrindex < meta.DataBlocks.Count) ? meta.DataBlocks[(int)ptrindex] : null;
                if ((ptrblock == null) || (ptrblock.Data == null))
                { break; } //not enough items..?
                if (ptrblock.StructureNameHash != name)
                { break; } //type mismatch..
            }

            return null;










            #region old version
            /* 
            //old version

            int currentp = (int)pointer - 1;
            var datablock = ((currentp>=0)&&(currentp < meta.DataBlocks.Count)) ? meta.DataBlocks[currentp] : null;
            if ((datablock == null) || (datablock.StructureNameHash != name)) //no match. look for the first match
            {
                currentp = -1;
                for (int i = 0; i < meta.DataBlocks.Count; i++)
                {
                    if (meta.DataBlocks[i].StructureNameHash == name)
                    {
                        currentp = i;
                        break;
                    }
                }
                if (currentp < 0) //couldn't find the right type.
                {
                    return null;
                }
            }

            int curindex = 0;
            while (itemsleft > 0)
            {
                if ((currentp >= meta.DataBlocks.Count) || (meta.DataBlocks[currentp].StructureNameHash != name))
                {
                    //wasn't enough data to read... problem.. (no next block to read from), just return whatever we got
                    T[] newitems = new T[curindex]; //have to return a smaller array..
                    for (int n = 0; n < curindex; n++) newitems[n] = items[n];
                    return newitems;
                }
                datablock = meta.DataBlocks[currentp];
                int curitemcount = datablock.DataLength / itemsize;
                int totusedbytes = curitemcount * itemsize;
                if (totusedbytes < datablock.DataLength)
                {
                    //didn't use all the bytes in the block... potential problem!
                    if (totusedbytes == 0)
                    {
                        //not big enough for one item..
                        if (curindex == 0) return null; //nothing read if on first iteration.
                        T[] newitems = new T[curindex]; //have to return a smaller array.. just return whatever we got
                        for (int n = 0; n < curindex; n++) newitems[n] = items[n];
                        return newitems;
                    }
                    else //there was something... how to know if it's right though??
                    {
                        //break; //just keep going.. at least parse what we got
                    }
                }

                for (int i = 0; i < curitemcount; i++)
                {
                    int offset = i * itemsize;
                    int index = curindex + i;
                    if (index >= items.Length)
                    {
                        //too much data! grow the output?
                        break;
                    }
                    items[index] = ConvertData<T>(datablock.Data, offset);
                }

                itemsleft -= curitemcount;
                curindex += curitemcount;
                currentp++;
            }



            return items;

            */
            #endregion
        }

        public static MetaPOINTER[] GetPointerArray(Meta meta, Array_StructurePointer array)
        {
            uint count = array.Count1;
            if (count == 0) return null;

            MetaPOINTER[] ptrs = new MetaPOINTER[count];
            int ptrsize = Marshal.SizeOf(typeof(MetaPOINTER));
            int ptroffset = (int)array.PointerDataOffset;
            var ptrblock = meta.GetBlock((int)array.PointerDataId);
            if ((ptrblock == null) || (ptrblock.Data == null) || (ptrblock.StructureNameHash != MetaName.POINTER))
            { return null; }

            for (int i = 0; i < count; i++)
            {
                int offset = ptroffset + (i * ptrsize);
                if (offset >= ptrblock.Data.Length)
                { break; }
                ptrs[i] = ConvertData<MetaPOINTER>(ptrblock.Data, offset);
            }

            return ptrs;
        }

        public static MetaHash[] GetHashArray(Meta meta, Array_uint array)
        {
            return ConvertDataArray<MetaHash>(meta, MetaName.HASH, array.Pointer, array.Count1);
        }
        public static Vector4[] GetPaddedVector3Array(Meta meta, Array_Vector3 array)
        {
            return ConvertDataArray<Vector4>(meta, MetaName.VECTOR4, array.Pointer, array.Count1);
        }
        public static uint[] GetUintArray(Meta meta, Array_uint array)
        {
            return ConvertDataArray<uint>(meta, MetaName.UINT, array.Pointer, array.Count1);
        }
        public static ushort[] GetUshortArray(Meta meta, Array_ushort array)
        {
            return ConvertDataArray<ushort>(meta, MetaName.USHORT, array.Pointer, array.Count1);
        }
        public static short[] GetShortArray(Meta meta, Array_ushort array)
        {
            return ConvertDataArray<short>(meta, MetaName.USHORT, array.Pointer, array.Count1);
        }
        public static float[] GetFloatArray(Meta meta, Array_float array)
        {
            return ConvertDataArray<float>(meta, MetaName.FLOAT, array.Pointer, array.Count1);
        }
        public static byte[] GetByteArray(Meta meta, Array_byte array)
        {
            var pointer = array.Pointer;
            uint ptrindex = (pointer & 0xFFF) - 1;
            uint ptroffset = ((pointer >> 12) & 0xFFFFF);
            var ptrblock = (ptrindex < meta.DataBlocks.Count) ? meta.DataBlocks[(int)ptrindex] : null;
            if ((ptrblock == null) || (ptrblock.Data == null))// || (ptrblock.StructureNameHash != name))
            { return null; } //no block or wrong block? shouldn't happen!
            var count = array.Count1;
            if ((ptroffset + count) > ptrblock.Data.Length)
            { return null; }
            byte[] data = new byte[count];
            Buffer.BlockCopy(ptrblock.Data, (int)ptroffset, data, 0, count);
            return data;
        }


        public static T[] GetTypedDataArray<T>(Meta meta, MetaName name) where T : struct
        {
            if ((meta == null) || (meta.DataBlocks == null)) return null;

            var datablocks = meta.DataBlocks.Data;

            MetaDataBlock startblock = null;
            int startblockind = -1;
            for (int i = 0; i < datablocks.Count; i++)
            {
                var block = datablocks[i];
                if (block.StructureNameHash == name)
                {
                    startblock = block;
                    startblockind = i;
                    break;
                }
            }
            if (startblock == null)
            {
                return null; //couldn't find the data.
            }

            int count = 0; //try figure out how many items there are, from the block size(s).
            int itemsize = Marshal.SizeOf(typeof(T));
            var currentblock = startblock;
            int currentblockind = startblockind;
            while (currentblock != null)
            {
                int blockitems = currentblock.DataLength / itemsize;
                count += blockitems;
                currentblockind++;
                if (currentblockind >= datablocks.Count) break; //last block, can't go any further
                currentblock = datablocks[currentblockind];
                if (currentblock.StructureNameHash != name) break; //not the right block type, can't go further
            }

            if (count <= 0)
            {
                return null; //didn't find anything...
            }

            return ConvertDataArray<T>(meta, name, (uint)startblockind + 1, (uint)count);
        }
        public static T GetTypedData<T>(Meta meta, MetaName name) where T : struct
        {
            foreach (var block in meta.DataBlocks)
            {
                if (block.StructureNameHash == name)
                {
                    return MetaTypes.ConvertData<T>(block.Data);
                }
            }
            throw new Exception("Couldn't find " + name.ToString() + " block.");
        }
        public static string[] GetStrings(Meta meta)
        {
            //look for strings in the sectionSTRINGS data block(s)

            if ((meta == null) || (meta.DataBlocks == null)) return null;

            var datablocks = meta.DataBlocks.Data;

            MetaDataBlock startblock = null;
            int startblockind = -1;
            for (int i = 0; i < datablocks.Count; i++)
            {
                var block = datablocks[i];
                if (block.StructureNameHash == MetaName.STRING)
                {
                    startblock = block;
                    startblockind = i;
                    break;
                }
            }
            if (startblock == null)
            {
                return null; //couldn't find the strings data section.
            }

            List<string> strings = new List<string>();
            StringBuilder sb = new StringBuilder();
            var currentblock = startblock;
            int currentblockind = startblockind;
            while (currentblock != null)
            {
                //read strings from the block.

                sb.Clear();
                int startindex = 0;
                int endindex = 0;
                var data = currentblock.Data;
                for (int b = 0; b < data.Length; b++)
                {
                    if (data[b] == 0)
                    {
                        startindex = endindex;
                        endindex = b;
                        if (endindex > startindex)
                        {
                            string str = Encoding.ASCII.GetString(data, startindex, endindex - startindex);
                            strings.Add(str);
                            endindex++; //start next string after the 0.
                        }
                    }
                }
                if (endindex != data.Length - 1)
                {
                    startindex = endindex;
                    endindex = data.Length - 1;
                    if (endindex > startindex)
                    {
                        string str = Encoding.ASCII.GetString(data, startindex, endindex - startindex);
                        strings.Add(str);
                    }
                }

                currentblockind++;
                if (currentblockind >= datablocks.Count) break; //last block, can't go any further
                currentblock = datablocks[currentblockind];
                if (currentblock.StructureNameHash != MetaName.STRING) break; //not the right block type, can't go further
            }


            if (strings.Count <= 0)
            {
                return null; //don't return empty array...
            }
            return strings.ToArray();
        }
        public static string GetString(Meta meta, CharPointer ptr)
        {
            var blocki = (int)ptr.PointerDataIndex;// (ptr.Pointer & 0xFFF) - 1;
            var offset = (int)ptr.PointerDataOffset;// (ptr.Pointer >> 12) & 0xFFFFF;
            if ((blocki < 0) || (blocki >= meta.DataBlocks.BlockLength))
            { return null; }
            var block = meta.DataBlocks[blocki];
            if (block.StructureNameHash != MetaName.STRING)
            { return null; }
            //var byteoffset = offset * 16 + offset2;
            var length = ptr.Count1;
            var lastbyte = offset + length;
            if (lastbyte >= block.DataLength)
            { return null; }
            string s = Encoding.ASCII.GetString(block.Data, offset, length);

            //if (meta.Strings == null) return null;
            //if (offset < 0) return null;
            //if (offset >= meta.Strings.Length) return null;
            //string s = meta.Strings[offset];

            return s;
        }

        public static MetaWrapper[] GetExtensions(Meta meta, Array_StructurePointer ptr)
        {
            if (ptr.Count1 == 0) return null;
            var result = new MetaWrapper[ptr.Count1];
            var extptrs = GetPointerArray(meta, ptr);
            if (extptrs != null)
            {
                for (int i = 0; i < extptrs.Length; i++)
                {
                    var extptr = extptrs[i];
                    MetaWrapper ext = null;
                    var block = meta.GetBlock(extptr.BlockID);
                    var h = block.StructureNameHash;
                    switch (h)
                    {
                        //archetype extension types
                        case MetaName.CExtensionDefParticleEffect:
                            ext = new MCExtensionDefParticleEffect();// MetaExtension<CExtensionDefParticleEffect>(h, GetData<CExtensionDefParticleEffect>(block, extptr));
                            break;
                        case MetaName.CExtensionDefAudioCollisionSettings:
                            ext = new MCExtensionDefAudioCollisionSettings();// MetaExtension<CExtensionDefAudioCollisionSettings>(h, GetData<CExtensionDefAudioCollisionSettings>(block, extptr));
                            break;
                        case MetaName.CExtensionDefAudioEmitter:
                            ext = new MCExtensionDefAudioEmitter();// MetaExtension<CExtensionDefAudioEmitter>(h, GetData<CExtensionDefAudioEmitter>(block, extptr));
                            break;
                        case MetaName.CExtensionDefSpawnPoint:
                            ext = new MCExtensionDefSpawnPoint();// new MetaExtension<CExtensionDefSpawnPoint>(h, GetData<CExtensionDefSpawnPoint>(block, extptr));
                            break;
                        case MetaName.CExtensionDefExplosionEffect:
                            ext = new MCExtensionDefExplosionEffect();// MetaExtension<CExtensionDefExplosionEffect>(h, GetData<CExtensionDefExplosionEffect>(block, extptr));
                            break;
                        case MetaName.CExtensionDefLadder:
                            ext = new MCExtensionDefLadder();// MetaExtension<CExtensionDefLadder>(h, GetData<CExtensionDefLadder>(block, extptr));
                            break;
                        case MetaName.CExtensionDefBuoyancy:
                            ext = new MCExtensionDefBuoyancy();// MetaExtension<CExtensionDefBuoyancy>(h, GetData<CExtensionDefBuoyancy>(block, extptr));
                            break;
                        case MetaName.CExtensionDefExpression:
                            ext = new MCExtensionDefExpression();// MetaExtension<CExtensionDefExpression>(h, GetData<CExtensionDefExpression>(block, extptr));
                            break;
                        case MetaName.CExtensionDefLightShaft:
                            ext = new MCExtensionDefLightShaft();// MetaExtension<CExtensionDefLightShaft>(h, GetData<CExtensionDefLightShaft>(block, extptr));
                            break;
                        case MetaName.CExtensionDefWindDisturbance:
                            ext = new MCExtensionDefWindDisturbance();// MetaExtension<CExtensionDefWindDisturbance>(h, GetData<CExtensionDefWindDisturbance>(block, extptr));
                            break;
                        case MetaName.CExtensionDefProcObject:
                            ext = new MCExtensionDefProcObject();// MetaExtension<CExtensionDefProcObject>(h, GetData<CExtensionDefProcObject>(block, extptr));
                            break;

                        //entity extension types
                        case MetaName.CExtensionDefLightEffect:
                            ext = new MCExtensionDefLightEffect();// MetaExtension<CExtensionDefLightEffect>(h, GetData<CExtensionDefLightEffect>(block, extptr));
                            break;
                        case MetaName.CExtensionDefSpawnPointOverride:
                            ext = new MCExtensionDefSpawnPointOverride();// MetaExtension<CExtensionDefSpawnPointOverride>(h, GetData<CExtensionDefSpawnPointOverride>(block, extptr));
                            break;
                        case MetaName.CExtensionDefDoor:
                            ext = new MCExtensionDefDoor();// MetaExtension<CExtensionDefDoor>(h, GetData<CExtensionDefDoor>(block, extptr));
                            break;
                        case MetaName.rage__phVerletClothCustomBounds: //rage__phVerletClothCustomBounds
                            ext = new Mrage__phVerletClothCustomBounds();// MetaExtension<rage__phVerletClothCustomBounds>(h, GetData<rage__phVerletClothCustomBounds>(block, extptr));
                            break;

                        default:
                            break;
                    }

                    //string ts = GetTypesInitString(meta);

                    if (ext != null)
                    {
                        ext.Load(meta, extptr);
                    }
                    if (i < result.Length)
                    {
                        result[i] = ext;
                    }
                }
            }
            return result;
        }


        public static int GetDataOffset(MetaDataBlock block, MetaPOINTER ptr)
        {
            if (block == null) return -1;
            var offset = ptr.Offset;
            if (ptr.ExtraOffset != 0)
            { }
            //offset += (int)ptr.ExtraOffset;
            if ((offset < 0) || (block.Data == null) || (offset >= block.Data.Length))
            { return -1; }
            return offset;
        }
        public static T GetData<T>(Meta meta, MetaPOINTER ptr) where T : struct
        {
            var block = meta.GetBlock(ptr.BlockID);
            var offset = GetDataOffset(block, ptr);
            if (offset < 0) return new T();
            return ConvertData<T>(block.Data, offset);
        }


        public static ushort SwapBytes(ushort x)
        {
            return (ushort)(((x & 0xFF00) >> 8) | ((x & 0x00FF) << 8));
        }
        public static uint SwapBytes(uint x)
        {
            // swap adjacent 16-bit blocks
            x = (x >> 16) | (x << 16);
            // swap adjacent 8-bit blocks
            return ((x & 0xFF00FF00) >> 8) | ((x & 0x00FF00FF) << 8);
        }
        public static int SwapBytes(int x)
        {
            return (int)SwapBytes((uint)x);
        }
        public static ulong SwapBytes(ulong x)
        {
            // swap adjacent 32-bit blocks
            x = (x >> 32) | (x << 32);
            // swap adjacent 16-bit blocks
            x = ((x & 0xFFFF0000FFFF0000) >> 16) | ((x & 0x0000FFFF0000FFFF) << 16);
            // swap adjacent 8-bit blocks
            return ((x & 0xFF00FF00FF00FF00) >> 8) | ((x & 0x00FF00FF00FF00FF) << 8);
        }
        public static float SwapBytes(float f)
        {
            var a = BitConverter.GetBytes(f);
            Array.Reverse(a);
            return BitConverter.ToSingle(a, 0);
        }
        public static Vector2 SwapBytes(Vector2 v)
        {
            var x = SwapBytes(v.X);
            var y = SwapBytes(v.Y);
            return new Vector2(x, y);
        }
        public static Vector3 SwapBytes(Vector3 v)
        {
            var x = SwapBytes(v.X);
            var y = SwapBytes(v.Y);
            var z = SwapBytes(v.Z);
            return new Vector3(x, y, z);
        }
        public static Vector4 SwapBytes(Vector4 v)
        {
            var x = SwapBytes(v.X);
            var y = SwapBytes(v.Y);
            var z = SwapBytes(v.Z);
            var w = SwapBytes(v.W);
            return new Vector4(x, y, z, w);
        }
    }




    [TC(typeof(EXP))] public abstract class MetaWrapper
    {
        public virtual string Name { get { return ToString(); } }
        public abstract void Load(Meta meta, MetaPOINTER ptr);
        public abstract MetaPOINTER Save(MetaBuilder mb);
    }




    //generated enums
    
    [Flags] public enum Unk_700327466 //SCENARIO point flags  / extension spawn point flags
        : int //Key:2814596095
    {
        IgnoreMaxInRange = 1,//0,
        NoSpawn = 2,//1,
        StationaryReactions = 4,//2,
        Unk_3257836369 = 8,//3,
        Unk_2165609255 = 16,//4,
        ActivateVehicleSiren = 32,//5,
        AggressiveVehicleDriving = 64,//6,
        Unk_2004780781 = 128,//7,
        Unk_536864854 = 256,//8,
        Unk_3441065168 = 512,//9,
        AerialVehiclePoint = 1024,//10,
        TerritorialScenario = 2048,//11,
        Unk_3690227693 = 4096,//12,
        Unk_1601179199 = 8192,//13,
        Unk_2583152330 = 16384,//14,
        Unk_3490317520 = 32768,//15,
        InWater = 65536,//16,
        Unk_1269249358 = 131072,//17,               // AllowInvestigation ?
        OpenDoor = 262144,//18,
        PreciseUseTime = 524288,//19,
        Unk_2247631388 = 1048576,//20,
        Unk_4100708934 = 2097152,//21,
        ExtendedRange = 4194304,//22,
        ShortRange = 8388608,//23,
        HighPriority = 16777216,//24,
        IgnoreLoitering = 33554432,//25,
        UseSearchlight = 67108864,//26,
        ResetNoCollisionOnCleanUp = 134217728,//27,
        Unk_3304563391 = 268435456,//28,
        Unk_1111379709 = 536870912,//29,
        IgnoreWeatherRestrictions = 1073741824,//30,
    }

    public enum Unk_3573596290 //SCENARIO Spawn point availability availableInMpSp
        : int //Key:671739257
    {
        kBoth = 0,
        kOnlySp = 1,
        kOnlyMp = 2,
    }

    public enum Unk_3609807418 //SCENARIO (Path) Edge Action
        : byte //Key:3326075799
    {
        Move = 0,
        Unk_7865678 = 1,
        MoveFollowMaster = 2,
    }

    public enum Unk_3971773454 //SCENARIO (Path) Edge nav mode
        : byte //Key:3016128742
    {
        Direct = 0,
        NavMesh = 1,
        Roads = 2,
    }

    public enum Unk_941086046 //SCENARIO (Path) Edge nav speed
        : byte //Key:1112851290
    {
        Unk_00_3279574318 = 0,
        Unk_01_2212923970 = 1,
        Unk_02_4022799658 = 2,
        Unk_03_1425672334 = 3,
        Unk_04_957720931 = 4,
        Unk_05_3795195414 = 5,
        Unk_06_2834622009 = 6,
        Unk_07_1876554076 = 7,
        Unk_08_698543797 = 8,
        Unk_09_1544199634 = 9,
        Unk_10_2725613303 = 10,
        Unk_11_4033265820 = 11,
        Unk_12_3054809929 = 12,
        Unk_13_3911005380 = 13,
        Unk_14_3717649022 = 14,
        Unk_15_3356026130 = 15,
    }

    public enum Unk_1991964615 //archetype assetType
        : int //Key:1866031916
    {
        ASSET_TYPE_UNINITIALIZED = 0, //189734893
        ASSET_TYPE_FRAGMENT = 1, //571047911
        ASSET_TYPE_DRAWABLE = 2, //130075505
        ASSET_TYPE_DRAWABLEDICTIONARY = 3, //1580165652
        ASSET_TYPE_ASSETLESS = 4, //4161085041
    }

    public enum Unk_1264241711 //entity lodLevel
        : int //Key:1856311430
    {
        LODTYPES_DEPTH_HD = 0,
        LODTYPES_DEPTH_LOD = 1,
        LODTYPES_DEPTH_SLOD1 = 2,
        LODTYPES_DEPTH_SLOD2 = 3,
        LODTYPES_DEPTH_SLOD3 = 4, //thanks Tadden :D
        LODTYPES_DEPTH_ORPHANHD = 5,
        LODTYPES_DEPTH_SLOD4 = 6,
    }

    public enum Unk_648413703 //entity priorityLevel
        : int //Key:2200357711
    {
        PRI_REQUIRED = 0,  //1943361227
        PRI_OPTIONAL_HIGH = 1, //3993616791
        PRI_OPTIONAL_MEDIUM = 2, //515598709
        PRI_OPTIONAL_LOW = 3, //329627604
    }

    public enum Unk_1294270217 //archetype CExtensionDefLadder materialType
        : int //Key:3514570158
    {
        METAL_SOLID_LADDER = 0, //Unk_1101797524 = 0,
        METAL_LIGHT_LADDER = 1,
        Unk_3202617440 = 2,
    }

    public enum Unk_1931949281 //archetype CExtensionDefLightShaft densityType
        : int //Key:3539601182
    {
        Unk_676250331 = 0,
        Unk_2399586564 = 1,
        Unk_2057886646 = 2,
        Unk_1816804348 = 3,
        Unk_152140774 = 4,
        Unk_2088805984 = 5,
        Unk_1098824079 = 6,
        Unk_1492299290 = 7,
    }

    public enum Unk_2266515059 //archetype CExtensionDefLightShaft volumeType
        : int //Key:4287472345
    {
        Unk_665241531 = 0,
        Unk_462992848 = 1,
    }

    public enum Unk_884254308 //component peds CComponentInfo ped accessory / variations slot
        : short //Key:3472084374
    {
        PV_COMP_INVALID = -1,
        PV_COMP_HEAD = 0,
        PV_COMP_BERD = 1,
        PV_COMP_HAIR = 2,
        PV_COMP_UPPR = 4,//3,
        PV_COMP_LOWR = 8,//4,
        PV_COMP_HAND = 16,//5,
        PV_COMP_FEET = 32,//6,
        PV_COMP_TEEF = 64,//7,
        PV_COMP_ACCS = 128,//8,
        PV_COMP_TASK = 256,//9,
        PV_COMP_DECL = 512,//10,
        PV_COMP_JBIB = 1024,//11,
        PV_COMP_MAX = 2048,//12,
    }

    public enum Unk_4212977111 //component peds Unk_94549140 renderFlags
        : int //Key:1551913633
    {
        Unk_3757767268 = 0,
        Unk_3735238938 = 1,
        Unk_3395845123 = 2,
    }

    public enum Unk_2834549053 //component peds CAnchorProps anchor
        : int //Key:1309372691
    {
        ANCHOR_HEAD = 0,
        ANCHOR_EYES = 1,
        ANCHOR_EARS = 2,
        ANCHOR_MOUTH = 3,
        ANCHOR_LEFT_HAND = 4,
        ANCHOR_RIGHT_HAND = 5,
        ANCHOR_LEFT_WRIST = 6,
        ANCHOR_RIGHT_WRIST = 7,
        ANCHOR_HIP = 8,
        ANCHOR_LEFT_FOOT = 9,
        ANCHOR_RIGHT_FOOT = 10,
        Unk_604819740 = 11,
        Unk_2358626934 = 12,
        NUM_ANCHORS = 13,
    }

    public enum Unk_3044470860 //cloth collision data SectionUNKNOWN1/1701774085 Flags
        : int //Key:1585854303
    {
        Unk_997866013 = 0,
    }






    //generated + adjusted structs code (UnusedX padding vars manually added) from here down, + meta wrapper classes

    [TC(typeof(EXP))] public struct CMapTypes //80 bytes, Key:2608875220
    {
        public uint Unused0 { get; set; }//0
        public uint Unused1 { get; set; }//4
        public Array_StructurePointer extensions { get; set; } //8   8: Array: 0: extensions  {0: StructurePointer: 0: 256}
        public Array_StructurePointer archetypes { get; set; } //24   24: Array: 0: archetypes  {0: StructurePointer: 0: 256}
        public MetaHash name { get; set; } //40   40: Hash: 0: name
        public uint Unused2 { get; set; }//44
        public Array_uint dependencies { get; set; } //48   48: Array: 0: dependencies//1013942340  {0: Hash: 0: 256}
        public Array_Structure compositeEntityTypes { get; set; } //64   64: Array: 0: compositeEntityTypes  {0: Structure: SectionUNKNOWN2: 256}

        public override string ToString()
        {
            return name.ToString();
        }
    }

    [TC(typeof(EXP))] public struct CBaseArchetypeDef //144 bytes, Key:2411387556
    {
        public uint Unused00 { get; set; }//0
        public uint Unused01 { get; set; }//4
        public float lodDist { get; set; } //8   8: Float: 0: lodDist
        public uint flags { get; set; } //12   12: UnsignedInt: 0: flags
        public uint specialAttribute { get; set; } //16   16: UnsignedInt: 0: specialAttribute
        public uint Unused02 { get; set; }//20
        public uint Unused03 { get; set; }//24
        public uint Unused04 { get; set; }//28
        public Vector3 bbMin { get; set; } //32   32: Float_XYZ: 0: bbMin
        public float Unused05 { get; set; }//44
        public Vector3 bbMax { get; set; } //48   48: Float_XYZ: 0: bbMax
        public float Unused06 { get; set; }//60
        public Vector3 bsCentre { get; set; } //64   64: Float_XYZ: 0: bsCentre
        public float Unused07 { get; set; }//76
        public float bsRadius { get; set; } //80   80: Float: 0: bsRadius
        public float hdTextureDist { get; set; } //84   84: Float: 0: hdTextureDist//2908576588
        public MetaHash name { get; set; } //88   88: Hash: 0: name
        public MetaHash textureDictionary { get; set; } //92   92: Hash: 0: textureDictionary
        public MetaHash clipDictionary { get; set; } //96   96: Hash: 0: clipDictionary//424089489
        public MetaHash drawableDictionary { get; set; } //100   100: Hash: 0: drawableDictionary
        public MetaHash physicsDictionary { get; set; } //104   104: Hash: 0: physicsDictionary//3553040380
        public Unk_1991964615 assetType { get; set; } //108   108: IntEnum: 1991964615: assetType
        public MetaHash assetName { get; set; } //112   112: Hash: 0: assetName
        public uint Unused08 { get; set; }//116
        public Array_StructurePointer extensions { get; set; } //120   120: Array: 0: extensions  {0: StructurePointer: 0: 256}
        public uint Unused09 { get; set; }//136
        public uint Unused10 { get; set; }//140


        public override string ToString()
        {
            return name.ToString() + ", " +
                   assetName.ToString() + ", " +
                   drawableDictionary.ToString() + ", " +
                   textureDictionary.ToString();
        }
    }

    [TC(typeof(EXP))] public struct CBaseArchetypeDef_v2 //128 bytes, Key:2352343492  //old version...
    {
        public uint Unused00 { get; set; }//0
        public uint Unused01 { get; set; }//4
        public float lodDist { get; set; } //8   8: Float: 0: lodDist
        public uint flags { get; set; } //12   12: UnsignedInt: 0: flags
        public uint specialAttribute { get; set; } //16   16: UnsignedInt: 0: specialAttribute
        public uint Unused02 { get; set; }//20
        public uint Unused03 { get; set; }//24
        public uint Unused04 { get; set; }//28
        public Vector3 bbMin { get; set; } //32   32: Float_XYZ: 0: bbMin
        public float Unused05 { get; set; }//44
        public Vector3 bbMax { get; set; } //48   48: Float_XYZ: 0: bbMax
        public float Unused06 { get; set; }//60
        public Vector3 bsCentre { get; set; } //64   64: Float_XYZ: 0: bsCentre
        public float Unused07 { get; set; }//76
        public float bsRadius { get; set; } //80   80: Float: 0: bsRadius
        public float hdTextureDist { get; set; } //84   84: Float: 0: hdTextureDist//2908576588
        public MetaHash name { get; set; } //88   88: Hash: 0: name
        public MetaHash textureDictionary { get; set; } //92   92: Hash: 0: textureDictionary
        public MetaHash clipDictionary { get; set; } //96   96: Hash: 0: clipDictionary//424089489
        public MetaHash drawableDictionary { get; set; } //100   100: Hash: 0: drawableDictionary
        public MetaHash physicsDictionary { get; set; } //104   104: Hash: 0: physicsDictionary//3553040380
        public uint Unused08 { get; set; }//108
        public Array_StructurePointer extensions { get; set; } //112   112: Array: 0: extensions  {0: StructurePointer: 0: 256}
    }

    [TC(typeof(EXP))] public struct CTimeArchetypeDefData
    {
        public uint timeFlags { get; set; } //144   144: UnsignedInt: 0: timeFlags//2248791340
        public uint Unused11 { get; set; }//148
        public uint Unused12 { get; set; }//152
        public uint Unused13 { get; set; }//156
    }
    [TC(typeof(EXP))] public struct CTimeArchetypeDef //160 bytes, Key:2520619910
    {
        public CBaseArchetypeDef _BaseArchetypeDef;
        public CTimeArchetypeDefData _TimeArchetypeDef;
        public CBaseArchetypeDef BaseArchetypeDef { get { return _BaseArchetypeDef; } set { _BaseArchetypeDef = value; } }
        public CTimeArchetypeDefData TimeArchetypeDef { get { return _TimeArchetypeDef; } set { _TimeArchetypeDef = value; } }

        public override string ToString()
        {
            return _BaseArchetypeDef.ToString();
        }
    }

    [TC(typeof(EXP))] public struct CMloArchetypeDefData
    {
        public uint mloFlags { get; set; } //144   144: UnsignedInt: 0: mloFlags//3590839912
        public uint Unused11 { get; set; }//148
        public Array_StructurePointer entities { get; set; } //152   152: Array: 0: entities  {0: StructurePointer: 0: 256}
        public Array_Structure rooms { get; set; } //168   168: Array: 0: rooms  {0: Structure: CMloRoomDef: 256}
        public Array_Structure portals { get; set; } //184   184: Array: 0: portals//2314725778  {0: Structure: CMloPortalDef: 256}
        public Array_Structure entitySets { get; set; } //200   200: Array: 0: entitySets//1169996080  {0: Structure: CMloEntitySet: 256}
        public Array_Structure timeCycleModifiers { get; set; } //216   216: Array: 0: timeCycleModifiers  {0: Structure: CMloTimeCycleModifier: 256}
        public uint Unused12 { get; set; }//232
        public uint Unused13 { get; set; }//236
    }
    [TC(typeof(EXP))] public struct CMloArchetypeDef //240 bytes, Key:937664754
    {
        public CBaseArchetypeDef _BaseArchetypeDef;
        public CMloArchetypeDefData _MloArchetypeDef;
        public CBaseArchetypeDef BaseArchetypeDef { get { return _BaseArchetypeDef; } set { _BaseArchetypeDef = value; } }
        public CMloArchetypeDefData MloArchetypeDef { get { return _MloArchetypeDef; } set { _MloArchetypeDef = value; } }

        public override string ToString()
        {
            return _BaseArchetypeDef.ToString();
        }
    }

    [TC(typeof(EXP))] public struct CMloInstanceDef //160 bytes, Key:2151576752
    {
        public CEntityDef CEntityDef { get; set; }
        public uint groupId { get; set; } //128   128: UnsignedInt: 0: 2501631252
        public uint floorId { get; set; } //132   132: UnsignedInt: 0: floorId//2187650609
        public Array_uint defaultEntitySets { get; set; } //136   136: Array: 0: defaultEntitySets//1407157833  {0: Hash: 0: 256}
        public uint numExitPortals { get; set; } //152   152: UnsignedInt: 0: numExitPortals//528711607
        public uint MLOInstflags { get; set; } //156   156: UnsignedInt: 0: MLOInstflags//3761966250
    }

    [TC(typeof(EXP))] public struct CMloRoomDef //112 bytes, Key:3885428245
    {
        public uint Unused0 { get; set; }//0
        public uint Unused1 { get; set; }//4
        public CharPointer name { get; set; } //8   8: CharPointer: 0: name
        public uint Unused2 { get; set; }//24
        public uint Unused3 { get; set; }//28
        public Vector3 bbMin { get; set; } //32   32: Float_XYZ: 0: bbMin
        public float Unused4 { get; set; }//44
        public Vector3 bbMax { get; set; } //48   48: Float_XYZ: 0: bbMax
        public float Unused5 { get; set; }//60
        public float blend { get; set; } //64   64: Float: 0: blend
        public MetaHash timecycleName { get; set; } //68   68: Hash: 0: timecycleName//2724323497
        public MetaHash secondaryTimecycleName { get; set; } //72   72: Hash: 0: secondaryTimecycleName//3255324828
        public uint flags { get; set; } //76   76: UnsignedInt: 0: flags
        public uint portalCount { get; set; } //80   80: UnsignedInt: 0: portalCount//1105339827
        public int floorId { get; set; } //84   84: SignedInt: 0: floorId//2187650609
        public int Unk_552849982 { get; set; } //88   88: SignedInt: 0: exteriorVisibiltyDepth//552849982
        public uint Unused6 { get; set; }//92
        public Array_uint attachedObjects { get; set; } //96   96: Array: 0: attachedObjects//2382704940  {0: UnsignedInt: 0: 256}
    }
    [TC(typeof(EXP))] public class MCMloRoomDef : MetaWrapper
    {
        public CMloRoomDef _Data;
        public CMloRoomDef Data { get { return _Data; } }
        public string RoomName { get; set; }
        public uint[] AttachedObjects { get; set; }

        public MCMloRoomDef() { }
        public MCMloRoomDef(Meta meta, CMloRoomDef data)
        {
            _Data = data;
            RoomName = MetaTypes.GetString(meta, _Data.name);
            AttachedObjects = MetaTypes.GetUintArray(meta, _Data.attachedObjects);
        }

        public override void Load(Meta meta, MetaPOINTER ptr)
        {
            _Data = MetaTypes.GetData<CMloRoomDef>(meta, ptr);
            RoomName = MetaTypes.GetString(meta, _Data.name);
            AttachedObjects = MetaTypes.GetUintArray(meta, _Data.attachedObjects);
        }

        public override MetaPOINTER Save(MetaBuilder mb)
        {
            if (!string.IsNullOrEmpty(RoomName))
            {
                _Data.name = mb.AddStringPtr(RoomName);
            }
            else
            {
                _Data.name = new CharPointer();
            }

            if (AttachedObjects != null)
            {
                _Data.attachedObjects = mb.AddUintArrayPtr(AttachedObjects);
            }
            else
            {
                _Data.attachedObjects = new Array_uint();
            }

            mb.AddStructureInfo(MetaName.CMloRoomDef);
            return mb.AddItemPtr(MetaName.CMloRoomDef, _Data);
        }

        public override string Name
        {
            get
            {
                return RoomName;
            }
        }

        public override string ToString()
        {
            return RoomName;
        }
    }

    [TC(typeof(EXP))] public struct CMloPortalDef //64 bytes, Key:1110221513
    {
        public uint Unused0 { get; set; }//0
        public uint Unused1 { get; set; }//4
        public uint roomFrom { get; set; } //8   8: UnsignedInt: 0: 4101034749
        public uint roomTo { get; set; } //12   12: UnsignedInt: 0: 2607060513
        public uint flags { get; set; } //16   16: UnsignedInt: 0: flags
        public uint mirrorPriority { get; set; } //20   20: UnsignedInt: 0: 1185490713
        public uint opacity { get; set; } //24   24: UnsignedInt: 0: opacity
        public uint audioOcclusion { get; set; } //28   28: UnsignedInt: 0: 1093790004
        public Array_Vector3 corners { get; set; } //32   32: Array: 0: corners  {0: Float_XYZ: 0: 256}
        public Array_uint attachedObjects { get; set; } //48   48: Array: 0: attachedObjects//2382704940  {0: UnsignedInt: 0: 256}
    }
    [TC(typeof(EXP))] public class MCMloPortalDef : MetaWrapper
    {
        public CMloPortalDef _Data;
        public CMloPortalDef Data { get { return _Data; } }
        public Vector4[] Corners { get; set; }
        public uint[] AttachedObjects { get; set; }

        public MCMloPortalDef() { }
        public MCMloPortalDef(Meta meta, CMloPortalDef data)
        {
            _Data = data;
            Corners = MetaTypes.GetPaddedVector3Array(meta, _Data.corners);
            AttachedObjects = MetaTypes.GetUintArray(meta, _Data.attachedObjects);
        }

        public override void Load(Meta meta, MetaPOINTER ptr)
        {
            _Data = MetaTypes.GetData<CMloPortalDef>(meta, ptr);
            Corners = MetaTypes.GetPaddedVector3Array(meta, _Data.corners);
            AttachedObjects = MetaTypes.GetUintArray(meta, _Data.attachedObjects);
        }

        public override MetaPOINTER Save(MetaBuilder mb)
        {
            if (Corners!=null)
            {
                _Data.corners = mb.AddPaddedVector3ArrayPtr(Corners);
            }
            else
            {
                _Data.corners = new Array_Vector3();
            }

            if (AttachedObjects != null)
            {
                _Data.attachedObjects = mb.AddUintArrayPtr(AttachedObjects);
            }
            else
            {
                _Data.attachedObjects = new Array_uint();
            }

            mb.AddStructureInfo(MetaName.CMloPortalDef);
            return mb.AddItemPtr(MetaName.CMloPortalDef, _Data);
        }

        public override string Name
        {
            get
            {
                return _Data.roomFrom.ToString() + " to " + _Data.roomTo.ToString();
            }
        }

        public override string ToString()
        {
            return Name;
        }
    }

    [TC(typeof(EXP))] public struct CMloEntitySet //48 bytes, Key:4180211587
    {
        public uint Unused0 { get; set; }//0
        public uint Unused1 { get; set; }//4
        public MetaHash name { get; set; } //8   8: Hash: 0: name
        public uint Unused2 { get; set; }//12
        public Array_uint locations { get; set; } //16   16: Array: 0: locations  {0: UnsignedInt: 0: 256}
        public Array_StructurePointer entities { get; set; } //32   32: Array: 0: entities  {0: StructurePointer: 0: 256}
    }
    [TC(typeof(EXP))] public class MCMloEntitySet : MetaWrapper
    {
        public CMloEntitySet _Data;
        public CMloEntitySet Data { get { return _Data; } }
        public uint[] Locations { get; set; }
        public MCEntityDef[] Entities { get; set; }

        public MCMloEntitySet() { }
        public MCMloEntitySet(Meta meta, CMloEntitySet data)
        {
            _Data = data;
            Load(meta);
        }

        public override void Load(Meta meta, MetaPOINTER ptr)
        {
            _Data = MetaTypes.GetData<CMloEntitySet>(meta, ptr);
            Load(meta);
        }

        private void Load(Meta meta)
        {
            Locations = MetaTypes.GetUintArray(meta, _Data.locations);

            var ents = MetaTypes.ConvertDataArray<CEntityDef>(meta, MetaName.CEntityDef, _Data.entities);
            if (ents != null)
            {
                Entities = new MCEntityDef[ents.Length];
                for (int i = 0; i < ents.Length; i++)
                {
                    Entities[i] = new MCEntityDef(meta, ents[i]);
                }
            }
        }


        public override MetaPOINTER Save(MetaBuilder mb)
        {
            if (Locations != null)
            {
                _Data.locations = mb.AddUintArrayPtr(Locations);
            }
            else
            {
                _Data.locations = new Array_uint();
            }

            if (Entities!=null)
            {
                _Data.entities = mb.AddWrapperArrayPtr(Entities);
            }
            else
            {
                _Data.entities = new Array_StructurePointer();
            }

            mb.AddStructureInfo(MetaName.CMloEntitySet);
            return mb.AddItemPtr(MetaName.CMloEntitySet, _Data);
        }

        public override string Name
        {
            get
            {
                return _Data.name.ToString();
            }
        }

        public override string ToString()
        {
            return Name + ": " + (Entities?.Length ?? 0).ToString() + " entities";
        }
    }

    [TC(typeof(EXP))] public struct CMloTimeCycleModifier //48 bytes, Key:838874674
    {
        public uint Unused0 { get; set; }//0
        public uint Unused1 { get; set; }//4
        public MetaHash name { get; set; } //8   8: Hash: 0: name
        public uint Unused2 { get; set; }//12
        public Vector4 sphere { get; set; } //16   16: Float_XYZW: 0: sphere
        public float percentage { get; set; } //32   32: Float: 0: percentage
        public float range { get; set; } //36   36: Float: 0: range
        public uint startHour { get; set; } //40   40: UnsignedInt: 0: startHour
        public uint endHour { get; set; } //44   44: UnsignedInt: 0: vlink87812
    }





    [TC(typeof(EXP))] public struct CMapData //512 bytes, Key:3448101671
    {
        public uint Unused0 { get; set; }//0
        public uint Unused1 { get; set; }//4
        public MetaHash name { get; set; } //8   8: Hash: 0: name
        public MetaHash parent { get; set; } //12   12: Hash: 0: parent
        public uint flags { get; set; } //16   16: UnsignedInt: 0: flags
        public uint contentFlags { get; set; } //20   20: UnsignedInt: 0: contentFlags//1785155637
        public uint Unused2 { get; set; }//24
        public uint Unused3 { get; set; }//28
        public Vector3 streamingExtentsMin { get; set; } //32   32: Float_XYZ: 0: streamingExtentsMin//3710026271
        public float Unused4 { get; set; }//44
        public Vector3 streamingExtentsMax { get; set; } //48   48: Float_XYZ: 0: streamingExtentsMax//2720965429
        public float Unused5 { get; set; }//60
        public Vector3 entitiesExtentsMin { get; set; } //64   64: Float_XYZ: 0: entitiesExtentsMin//477478129
        public float Unused6 { get; set; }//76
        public Vector3 entitiesExtentsMax { get; set; } //80   80: Float_XYZ: 0: entitiesExtentsMax//1829192759
        public float Unused7 { get; set; }//92
        public Array_StructurePointer entities { get; set; } //96   96: Array: 0: entities  {0: StructurePointer: 0: 256}
        public Array_Structure containerLods { get; set; } //112   112: Array: 0: containerLods//2935983381  {0: Structure: 372253349: 256}
        public Array_Structure boxOccluders { get; set; } //128   128: Array: 0: boxOccluders//3983590932  {0: Structure: SectionUNKNOWN7: 256}
        public Array_Structure occludeModels { get; set; } //144   144: Array: 0: occludeModels//2132383965  {0: Structure: SectionUNKNOWN5: 256}
        public Array_uint physicsDictionaries { get; set; } //160   160: Array: 0: physicsDictionaries//949589348  {0: Hash: 0: 256}
        public rage__fwInstancedMapData instancedData { get; set; } //176   176: Structure: rage__fwInstancedMapData: instancedData//2569067561
        public Array_Structure timeCycleModifiers { get; set; } //224   224: Array: 0: timeCycleModifiers  {0: Structure: CTimeCycleModifier: 256}
        public Array_Structure carGenerators { get; set; } //240   240: Array: 0: carGenerators//3254823756  {0: Structure: CCarGen: 256}
        public CLODLight LODLightsSOA { get; set; } //256   256: Structure: CLODLight: LODLightsSOA//1774371066
        public CDistantLODLight DistantLODLightsSOA { get; set; } //392   392: Structure: CDistantLODLight: DistantLODLightsSOA//2954466641
        public CBlockDesc block { get; set; } //440   440: Structure: CBlockDesc//3072355914: block


        //notes:
        //CMapData.flags: 
        //  flag 1 = SCRIPTED flag  (eg destruction)
        //  flag 2 = LOD flag? reflection proxy flag?


        public override string ToString()
        {
            return name.ToString() + ": " + parent.ToString();
        }
    }

    [TC(typeof(EXP))] public struct CEntityDef //128 bytes, Key:1825799514
    {
        public uint Unused0 { get; set; }//0
        public uint Unused1 { get; set; }//4
        public MetaHash archetypeName { get; set; } //8   8: Hash: 0: archetypeName
        public uint flags { get; set; } //12   12: UnsignedInt: 0: flags
        public uint guid { get; set; } //16   16: UnsignedInt: 0: guid
        public uint Unused2 { get; set; }//20
        public uint Unused3 { get; set; }//24
        public uint Unused4 { get; set; }//28
        public Vector3 position { get; set; } //32   32: Float_XYZ: 0: position
        public float Unused5 { get; set; }//44
        public Vector4 rotation { get; set; } //48   48: Float_XYZW: 0: rotation
        public float scaleXY { get; set; } //64   64: Float: 0: 2627937847
        public float scaleZ { get; set; } //68   68: Float: 0: 284916802
        public int parentIndex { get; set; } //72   72: SignedInt: 0: parentIndex
        public float lodDist { get; set; } //76   76: Float: 0: lodDist
        public float childLodDist { get; set; } //80   80: Float: 0: childLodDist//3398912973
        public Unk_1264241711 lodLevel { get; set; } //84   84: IntEnum: 1264241711: lodLevel  //LODTYPES_DEPTH_
        public uint numChildren { get; set; } //88   88: UnsignedInt: 0: numChildren//2793909385
        public Unk_648413703 priorityLevel { get; set; } //92   92: IntEnum: 648413703: priorityLevel//647098393
        public Array_StructurePointer extensions { get; set; } //96   96: Array: 0: extensions  {0: StructurePointer: 0: 256}
        public int ambientOcclusionMultiplier { get; set; } //112   112: SignedInt: 0: ambientOcclusionMultiplier//415356295
        public int artificialAmbientOcclusion { get; set; } //116   116: SignedInt: 0: artificialAmbientOcclusion//599844163
        public uint tintValue { get; set; } //120   120: UnsignedInt: 0: tintValue//1015358759
        public uint Unused6 { get; set; }//124


        public override string ToString()
        {
            return JenkIndex.GetString(archetypeName) + ": " + JenkIndex.GetString(guid) + ": " + position.ToString();
        }
    }
    [TC(typeof(EXP))] public class MCEntityDef : MetaWrapper
    {
        public CEntityDef _Data;
        public CEntityDef Data { get { return _Data; } }
        public MetaWrapper[] Extensions { get; set; }


        public MCEntityDef() { }
        public MCEntityDef(MCEntityDef copy)
        {
            if (copy != null)
            {
                _Data = copy.Data;
            }
        }
        public MCEntityDef(Meta meta, CEntityDef d)
        {
            _Data = d;
            Extensions = MetaTypes.GetExtensions(meta, _Data.extensions);
        }

        public override void Load(Meta meta, MetaPOINTER ptr)
        {
            _Data = MetaTypes.GetData<CEntityDef>(meta, ptr);
            Extensions = MetaTypes.GetExtensions(meta, _Data.extensions);
        }

        public override MetaPOINTER Save(MetaBuilder mb)
        {
            if (Extensions != null)
            {
                _Data.extensions = mb.AddWrapperArrayPtr(Extensions);
            }
            else
            {
                _Data.extensions = new Array_StructurePointer();
            }

            mb.AddStructureInfo(MetaName.CEntityDef);
            return mb.AddItemPtr(MetaName.CEntityDef, _Data);
        }

        public override string Name
        {
            get
            {
                return _Data.archetypeName.ToString();
            }
        }

        public override string ToString()
        {
            return Name;
        }
    }

    [TC(typeof(EXP))] public struct Unk_975711773 //16 bytes, Key:1831736438   //boxOccluders
    {
        public short iCenterX { get; set; } //0   0: SignedShort: 0: 48026296
        public short iCenterY { get; set; } //2   2: SignedShort: 0: 896907229
        public short iCenterZ { get; set; } //4   4: SignedShort: 0: 1597508449
        public short iCosZ { get; set; } //6   6: SignedShort: 0: iCosZ
        public short iLength { get; set; } //8   8: SignedShort: 0: 2854610661
        public short iWidth { get; set; } //10   10: SignedShort: 0: 168013536
        public short iHeight { get; set; } //12   12: SignedShort: 0: 3485277993
        public short iSinZ { get; set; } //14   14: SignedShort: 0: iSinZ
    }

    [TC(typeof(EXP))] public struct Unk_2741784237 //64 bytes, Key:1172796107  //occludeModels
    {
        public Vector3 bmin { get; set; } //0   0: Float_XYZ: 0: bmin
        public float Unused0 { get; set; }//12
        public Vector3 bmax { get; set; } //16   16: Float_XYZ: 0: bmax
        public float Unused1 { get; set; }//28
        public uint dataSize { get; set; } //32   32: UnsignedInt: 0: dataSize//2442753371
        public uint Unused2 { get; set; }//36
        public DataBlockPointer verts { get; set; } //40   40: DataBlockPointer: 2: verts
        public ushort Unk_853977995 { get; set; } //48   48: UnsignedShort: 0: 853977995
        public ushort Unk_2337695078 { get; set; } //50   50: UnsignedShort: 0: 2337695078
        public uint flags { get; set; } //52   52: UnsignedInt: 0: flags
        public uint Unused3 { get; set; }//56
        public uint Unused4 { get; set; }//60
    }

    [TC(typeof(EXP))] public struct rage__fwInstancedMapData //48 bytes, Key:1836780118
    {
        public uint Unused0 { get; set; }//0
        public uint Unused1 { get; set; }//4
        public MetaHash ImapLink { get; set; } //8   8: Hash: 0: ImapLink//2142127586
        public uint Unused2 { get; set; }//12
        public Array_Structure PropInstanceList { get; set; } //16   16: Array: 0: PropInstanceList//3551474528  {0: Structure: rage__fwPropInstanceListDef: 256}
        public Array_Structure GrassInstanceList { get; set; } //32   32: Array: 0: GrassInstanceList//255292381  {0: Structure: rage__fwGrassInstanceListDef: 256}

        public override string ToString()
        {
            return ImapLink.ToString() + ", " + PropInstanceList.Count1.ToString() + " props, " + GrassInstanceList.Count1.ToString() + " grasses";
        }
    }

    [TC(typeof(EXP))] public struct rage__fwGrassInstanceListDef //96 bytes, Key:941808164  rage__fwGrassInstanceListDef//2085051229
    {
        public rage__spdAABB BatchAABB { get; set; } //0   0: Structure: 4084721864: BatchAABB//1859041902
        public Vector3 ScaleRange { get; set; } //32   32: Float_XYZ: 0: ScaleRange
        public float Unused0 { get; set; }//44
        public MetaHash archetypeName { get; set; } //48   48: Hash: 0: archetypeName
        public uint lodDist { get; set; } //52   52: UnsignedInt: 0: lodDist
        public float LodFadeStartDist { get; set; } //56   56: Float: 0: LodFadeStartDist//2216273066
        public float LodInstFadeRange { get; set; } //60   60: Float: 0: LodInstFadeRange//1405992723
        public float OrientToTerrain { get; set; } //64   64: Float: 0: OrientToTerrain//3341475578
        public uint Unused1 { get; set; }//68
        public Array_Structure InstanceList { get; set; } //72   72: Array: 0: InstanceList//470289337  {0: Structure: rage__fwGrassInstanceListDef__InstanceData: 256}
        public uint Unused2 { get; set; }//88
        public uint Unused3 { get; set; }//92

        public override string ToString()
        {
            return archetypeName.ToString() + " (" + InstanceList.Count1.ToString() + " instances)";
        }
    }

    [TC(typeof(EXP))] public struct rage__fwGrassInstanceListDef__InstanceData //16 bytes, Key:2740378365 rage__fwGrassInstanceListDef__InstanceData//3985044770 // Tom: Something to do with placing foliage
    {
        public ArrayOfUshorts3 Position { get; set; } //0   0: ArrayOfBytes: 3: Position - Ushorts
        public byte NormalX { get; set; } //6   6: UnsignedByte: 0: NormalX//3138065392
        public byte NormalY { get; set; } //7   7: UnsignedByte: 0: NormalY//273792636
        public ArrayOfBytes3 Color { get; set; } //8   8: ArrayOfBytes: 3: Color
        public byte Scale { get; set; } //11   11: UnsignedByte: 0: Scale
        public byte Ao { get; set; } //12   12: UnsignedByte: 0: Ao//2996378564
        public ArrayOfBytes3 Pad { get; set; } //13   13: ArrayOfBytes: 3: Pad

        public override string ToString()
        {
            return Position.ToString() + " : " + Color.ToString() + " : " + Scale.ToString();
        }
    }

    [TC(typeof(EXP))] public struct CTimeCycleModifier //64 bytes, Key:2683420777
    {
        public uint Unused0 { get; set; }//0
        public uint Unused1 { get; set; }//4
        public MetaHash name { get; set; } //8   8: Hash: 0: name  
        public uint Unused2 { get; set; }//12
        public Vector3 minExtents { get; set; } //16   16: Float_XYZ: 0: minExtents=1731020657
        public float Unused3 { get; set; }//28
        public Vector3 maxExtents { get; set; } //32   32: Float_XYZ: 0: maxExtents=2554806840
        public float Unused4 { get; set; }//44
        public float percentage { get; set; } //48   48: Float: 0: percentage
        public float range { get; set; } //52   52: Float: 0: range
        public uint startHour { get; set; } //56   56: UnsignedInt: 0: startHour
        public uint endHour { get; set; } //60   60: UnsignedInt: 0: endHour


        //regarding name in OpenIV:
        //2633803310 = NoAmbientmult
        //2003616884 = INT_NoAmbientMult



        public override string ToString()
        {
            return name.ToString() + ": startHour " + startHour.ToString() + ", endHour " + endHour.ToString() + ", range " + range.ToString() + ", percentage " + percentage.ToString();
        }
    }

    [TC(typeof(EXP))] public struct CCarGen //80 bytes, Key:2345238261
    {
        public uint Unused0 { get; set; }//0
        public uint Unused1 { get; set; }//4
        public uint Unused2 { get; set; }//8
        public uint Unused3 { get; set; }//12
        public Vector3 position { get; set; } //16   16: Float_XYZ: 0: position
        public float Unused4 { get; set; }//28
        public float orientX { get; set; } //32   32: Float: 0: orientX=735213009
        public float orientY { get; set; } //36   36: Float: 0: orientY=979440342
        public float perpendicularLength { get; set; } //40   40: Float: 0: perpendicularLength=124715667
        public MetaHash carModel { get; set; } //44   44: Hash: 0: carModel
        public uint flags { get; set; } //48   48: UnsignedInt: 0: flags   ///  _CP_: looks like flag 1879051873 in cargens forces to spawn a vehicle
        public int bodyColorRemap1 { get; set; } //52   52: SignedInt: 0: bodyColorRemap1=1429703670
        public int bodyColorRemap2 { get; set; } //56   56: SignedInt: 0: bodyColorRemap2=1254848286
        public int bodyColorRemap3 { get; set; } //60   60: SignedInt: 0: bodyColorRemap3=1880965569
        public int bodyColorRemap4 { get; set; } //64   64: SignedInt: 0: bodyColorRemap4=1719152247
        public MetaHash popGroup { get; set; } //68   68: Hash: 0: popGroup=911358791
        public sbyte livery { get; set; } //72   72: SignedByte: 0: livery
        public byte Unused5 { get; set; }//73
        public ushort Unused6 { get; set; }//74
        public uint Unused7 { get; set; }//76

        public override string ToString()
        {
            return carModel.ToString() + ", " + position.ToString() + ", " + popGroup.ToString() + ", " + livery.ToString();
        }
    }

    [TC(typeof(EXP))] public struct CLODLight //136 bytes, Key:2325189228
    {
        public uint Unused0 { get; set; }//0
        public uint Unused1 { get; set; }//4
        public Array_Structure direction { get; set; } //8   8: Array: 0: direction  {0: Structure: VECTOR3: 256}
        public Array_float falloff { get; set; } //24   24: Array: 0: falloff  {0: Float: 0: 256}
        public Array_float falloffExponent { get; set; } //40   40: Array: 0: falloffExponent  {0: Float: 0: 256}
        public Array_uint timeAndStateFlags { get; set; } //56   56: Array: 0: timeAndStateFlags=3112418278  {0: UnsignedInt: 0: 256}
        public Array_uint hash { get; set; } //72   72: Array: 0: hash  {0: UnsignedInt: 0: 256}
        public Array_byte coneInnerAngle { get; set; } //88   88: Array: 0: coneInnerAngle//1163671864  {0: UnsignedByte: 0: 256}
        public Array_byte coneOuterAngleOrCapExt { get; set; } //104   104: Array: 0: coneOuterAngleOrCapExt=3161894080  {0: UnsignedByte: 0: 256}
        public Array_byte coronaIntensity { get; set; } //120   120: Array: 0: coronaIntensity//2292363771  {0: UnsignedByte: 0: 256}
    }

    [TC(typeof(EXP))] public struct CDistantLODLight //48 bytes, Key:2820908419
    {
        public uint Unused0 { get; set; }//0
        public uint Unused1 { get; set; }//4
        public Array_Structure position { get; set; } //8   8: Array: 0: position  {0: Structure: VECTOR3: 256}
        public Array_uint RGBI { get; set; } //24   24: Array: 0: RGBI  {0: UnsignedInt: 0: 256}
        public ushort numStreetLights { get; set; } //40   40: UnsignedShort: 0: numStreetLights//3708891211
        public ushort category { get; set; } //42   42: UnsignedShort: 0: category//2052871693
        public uint Unused2 { get; set; }//44
    }

    [TC(typeof(EXP))] public struct CBlockDesc //72 bytes, Key:2015795449
    {
        public uint version { get; set; } //0   0: UnsignedInt: 0: version
        public uint flags { get; set; } //4   4: UnsignedInt: 0: flags
        public CharPointer name { get; set; } //8   8: CharPointer: 0: name
        public CharPointer exportedBy { get; set; } //24   24: CharPointer: 0: exportedBy//1983184981
        public CharPointer owner { get; set; } //40   40: CharPointer: 0: owner
        public CharPointer time { get; set; } //56   56: CharPointer: 0: time
    }










    [TC(typeof(EXP))] public struct CExtensionDefParticleEffect //96 bytes, Key:466596385
    {
        public uint Unused0 { get; set; }//0
        public uint Unused1 { get; set; }//4
        public MetaHash name { get; set; } //8   8: Hash: 0: name
        public uint Unused2 { get; set; }//12
        public Vector3 offsetPosition { get; set; } //16   16: Float_XYZ: 0: offsetPosition
        public uint Unused3 { get; set; }//28
        public Vector4 offsetRotation { get; set; } //32   32: Float_XYZW: 0: offsetRotation
        public CharPointer fxName { get; set; } //48   48: CharPointer: 0: fxName
        public int fxType { get; set; } //64   64: SignedInt: 0: fxType
        public int boneTag { get; set; } //68   68: SignedInt: 0: boneTag
        public float scale { get; set; } //72   72: Float: 0: scale
        public int probability { get; set; } //76   76: SignedInt: 0: probability
        public int flags { get; set; } //80   80: SignedInt: 0: flags
        public uint color { get; set; } //84   84: UnsignedInt: 0: color
        public uint Unused4 { get; set; }//88
        public uint Unused5 { get; set; }//92

        public override string ToString()
        {
            return "CExtensionDefParticleEffect - " + name.ToString();
        }
    }
    [TC(typeof(EXP))] public class MCExtensionDefParticleEffect : MetaWrapper
    {
        public CExtensionDefParticleEffect _Data;
        public CExtensionDefParticleEffect Data { get { return _Data; } }

        public string fxName { get; set; }

        public override void Load(Meta meta, MetaPOINTER ptr)
        {
            _Data = MetaTypes.GetData<CExtensionDefParticleEffect>(meta, ptr);
            fxName = MetaTypes.GetString(meta, _Data.fxName);
        }

        public override MetaPOINTER Save(MetaBuilder mb)
        {
            if (fxName != null)
            {
                _Data.fxName = mb.AddStringPtr(fxName);
            }

            mb.AddStructureInfo(MetaName.CExtensionDefParticleEffect);
            return mb.AddItemPtr(MetaName.CExtensionDefParticleEffect, _Data);
        }

        public override string Name
        {
            get
            {
                return fxName;
            }
        }

        public override string ToString()
        {
            return _Data.ToString() + " - " + fxName;
        }
    }

    [TC(typeof(EXP))] public struct CExtensionDefLightEffect //48 bytes, Key:2436199897
    {
        public uint Unused0 { get; set; }//0
        public uint Unused1 { get; set; }//4
        public MetaHash name { get; set; } //8   8: Hash: 0: name
        public uint Unused2 { get; set; }//12
        public Vector3 offsetPosition { get; set; } //16   16: Float_XYZ: 0: offsetPosition
        public float Unused3 { get; set; }//28
        public Array_Structure instances { get; set; } //32   32: Array: 0: 274177522  {0: Structure: CLightAttrDef: 256}

        public override string ToString()
        {
            return "CExtensionDefLightEffect - " + name.ToString();
        }
    }
    [TC(typeof(EXP))] public class MCExtensionDefLightEffect : MetaWrapper
    {
        public CExtensionDefLightEffect _Data;
        public CExtensionDefLightEffect Data { get { return _Data; } }

        public CLightAttrDef[] instances { get; set; }

        public override void Load(Meta meta, MetaPOINTER ptr)
        {
            _Data = MetaTypes.GetData<CExtensionDefLightEffect>(meta, ptr);
            instances = MetaTypes.ConvertDataArray<CLightAttrDef>(meta, MetaName.CLightAttrDef, _Data.instances);
        }

        public override MetaPOINTER Save(MetaBuilder mb)
        {
            if (instances != null)
            {
                _Data.instances = mb.AddItemArrayPtr(MetaName.CLightAttrDef, instances);
            }

            mb.AddStructureInfo(MetaName.CLightAttrDef);
            mb.AddStructureInfo(MetaName.CExtensionDefLightEffect);
            return mb.AddItemPtr(MetaName.CExtensionDefLightEffect, _Data);
        }

        public override string Name
        {
            get
            {
                return _Data.name.ToString();
            }
        }

        public override string ToString()
        {
            return _Data.ToString() + " (" + (instances?.Length ?? 0).ToString() + " Attributes)";
        }
    }

    [TC(typeof(EXP))] public struct CLightAttrDef //160 bytes, Key:2363260268
    {
        public uint Unused00 { get; set; }//0
        public uint Unused01 { get; set; }//4
        public Vector3 posn { get; set; } //8   8: ArrayOfBytes: 3: posn //actually ArrayOfFloats3 (Vector3!)
        public ArrayOfBytes3 colour { get; set; } //20   20: ArrayOfBytes: 3: colour
        public byte flashiness { get; set; } //23   23: UnsignedByte: 0: 3829693202
        public float intensity { get; set; } //24   24: Float: 0: intensity
        public uint flags { get; set; } //28   28: UnsignedInt: 0: flags
        public short boneTag { get; set; } //32   32: SignedShort: 0: boneTag
        public byte lightType { get; set; } //34   34: UnsignedByte: 0: 482065968
        public byte groupId { get; set; } //35   35: UnsignedByte: 0: 2501631252
        public uint timeFlags { get; set; } //36   36: UnsignedInt: 0: timeFlags//2248791340
        public float falloff { get; set; } //40   40: Float: 0: falloff
        public float falloffExponent { get; set; } //44   44: Float: 0: falloffExponent
        public Vector4 cullingPlane { get; set; } //48   48: ArrayOfBytes: 4: 1689591312 //48: 1689591312//cullingPlane - actually ArrayOfFloats4!
        public byte shadowBlur { get; set; } //64   64: UnsignedByte: 0: shadowBlur
        public byte padding1 { get; set; } //65   65: UnsignedByte: 0: padding1//3180641850
        public short padding2 { get; set; } //66   66: SignedShort: 0: padding2//2346113727
        public uint padding3 { get; set; } //68   68: UnsignedInt: 0: padding3//3521603295
        public float volIntensity { get; set; } //72   72: Float: 0: volIntensity//689780512
        public float volSizeScale { get; set; } //76   76: Float: 0: volSizeScale//2029533327
        public ArrayOfBytes3 volOuterColour { get; set; } //80   80: ArrayOfBytes: 3: volOuterColour//2283994062
        public byte lightHash { get; set; } //83   83: UnsignedByte: 0: lightHash//643049222
        public float volOuterIntensity { get; set; } //84   84: Float: 0: volOuterIntensity//3008198647
        public float coronaSize { get; set; } //88   88: Float: 0: coronaSize//1705000075
        public float volOuterExponent { get; set; } //92   92: Float: 0: volOuterExponent//2758849250
        public byte lightFadeDistance { get; set; } //96   96: UnsignedByte: 0: lightFadeDistance//1307926275
        public byte shadowFadeDistance { get; set; } //97   97: UnsignedByte: 0: shadowFadeDistance//1944267876
        public byte specularFadeDistance { get; set; } //98   98: UnsignedByte: 0: specularFadeDistance//4150887048
        public byte volumetricFadeDistance { get; set; } //99   99: UnsignedByte: 0: volumetricFadeDistance//2066998816
        public float shadowNearClip { get; set; } //100   100: Float: 0: shadowNearClip//954647178
        public float coronaIntensity { get; set; } //104   104: Float: 0: coronaIntensity//2292363771
        public float coronaZBias { get; set; } //108   108: Float: 0: coronaZBias//2520359283
        public Vector3 direction { get; set; } //112   112: ArrayOfBytes: 3: direction //actually ArrayOfFloats3 (Vector3!)
        public Vector3 tangent { get; set; } //124   124: ArrayOfBytes: 3: tangent //actually ArrayOfFloats3 (Vector3!)
        public float coneInnerAngle { get; set; } //136   136: Float: 0: coneInnerAngle//1163671864
        public float coneOuterAngle { get; set; } //140   140: Float: 0: coneOuterAngle//4175029060
        public Vector3 extents { get; set; } //144   144: ArrayOfBytes: 3: extents//759134656 //actually ArrayOfFloats3 (Vector3!)
        public uint projectedTextureKey { get; set; } //156   156: UnsignedInt: 0: projectedTextureKey//1076718994
    }

    [TC(typeof(EXP))] public struct CExtensionDefAudioCollisionSettings //48 bytes, Key:2701897500
    {
        public uint Unused0 { get; set; }//0
        public uint Unused1 { get; set; }//4
        public MetaHash name { get; set; } //8   8: Hash: 0: name
        public uint Unused2 { get; set; }//12
        public Vector3 offsetPosition { get; set; } //16   16: Float_XYZ: 0: offsetPosition
        public float Unused3 { get; set; }//28
        public MetaHash settings { get; set; } //32   32: Hash: 0: settings
        public uint Unused4 { get; set; }//36
        public uint Unused5 { get; set; }//40
        public uint Unused6 { get; set; }//44

        public override string ToString()
        {
            return "CExtensionDefAudioCollisionSettings - " + name.ToString();
        }
    }
    [TC(typeof(EXP))] public class MCExtensionDefAudioCollisionSettings : MetaWrapper
    {
        public CExtensionDefAudioCollisionSettings _Data;
        public CExtensionDefAudioCollisionSettings Data { get { return _Data; } }

        public override void Load(Meta meta, MetaPOINTER ptr)
        {
            _Data = MetaTypes.GetData<CExtensionDefAudioCollisionSettings>(meta, ptr);
        }

        public override MetaPOINTER Save(MetaBuilder mb)
        {
            mb.AddStructureInfo(MetaName.CExtensionDefAudioCollisionSettings);
            return mb.AddItemPtr(MetaName.CExtensionDefAudioCollisionSettings, _Data);
        }

        public override string Name
        {
            get
            {
                return _Data.name.ToString();
            }
        }

        public override string ToString()
        {
            return _Data.ToString();
        }
    }

    [TC(typeof(EXP))] public struct CExtensionDefAudioEmitter //64 bytes, Key:15929839
    {
        public uint Unused0 { get; set; }//0
        public uint Unused1 { get; set; }//4
        public MetaHash name { get; set; } //8   8: Hash: 0: name
        public uint Unused2 { get; set; }//12
        public Vector3 offsetPosition { get; set; } //16   16: Float_XYZ: 0: offsetPosition
        public float Unused3 { get; set; }//28
        public Vector4 offsetRotation { get; set; } //32   32: Float_XYZW: 0: offsetRotation
        public MetaHash effectHash { get; set; } //48   48: UnsignedInt: 0: effectHash//2982223448
        public uint Unused4 { get; set; }//52
        public uint Unused5 { get; set; }//56
        public uint Unused6 { get; set; }//60

        public override string ToString()
        {
            return "CExtensionDefAudioEmitter - " + name.ToString() + ": " + effectHash.ToString() + ": " + offsetPosition.ToString();
        }
    }
    [TC(typeof(EXP))] public class MCExtensionDefAudioEmitter : MetaWrapper
    {
        public CExtensionDefAudioEmitter _Data;
        public CExtensionDefAudioEmitter Data { get { return _Data; } }

        public override void Load(Meta meta, MetaPOINTER ptr)
        {
            _Data = MetaTypes.GetData<CExtensionDefAudioEmitter>(meta, ptr);
        }

        public override MetaPOINTER Save(MetaBuilder mb)
        {
            mb.AddStructureInfo(MetaName.CExtensionDefAudioEmitter);
            return mb.AddItemPtr(MetaName.CExtensionDefAudioEmitter, _Data);
        }

        public override string Name
        {
            get
            {
                return _Data.name.ToString();
            }
        }

        public override string ToString()
        {
            return _Data.ToString();
        }
    }

    [TC(typeof(EXP))] public struct CExtensionDefExplosionEffect //80 bytes, Key:2840366784
    {
        public uint Unused0 { get; set; }//0
        public uint Unused1 { get; set; }//4
        public MetaHash name { get; set; } //8   8: Hash: 0: name
        public uint Unused2 { get; set; }//12
        public Vector3 offsetPosition { get; set; } //16   16: Float_XYZ: 0: offsetPosition
        public float Unused3 { get; set; }//28
        public Vector4 offsetRotation { get; set; } //32   32: Float_XYZW: 0: offsetRotation
        public CharPointer explosionName { get; set; } //48   48: CharPointer: 0: explosionName//3301388915
        public int boneTag { get; set; } //64   64: SignedInt: 0: boneTag
        public int explosionTag { get; set; } //68   68: SignedInt: 0: explosionTag//2653034051
        public int explosionType { get; set; } //72   72: SignedInt: 0: explosionType//3379115010
        public uint flags { get; set; } //76   76: UnsignedInt: 0: flags

        public override string ToString()
        {
            return "CExtensionDefExplosionEffect - " + name.ToString();
        }
    }
    [TC(typeof(EXP))] public class MCExtensionDefExplosionEffect : MetaWrapper
    {
        public CExtensionDefExplosionEffect _Data;
        public CExtensionDefExplosionEffect Data { get { return _Data; } }

        public string explosionName { get; set; }

        public override void Load(Meta meta, MetaPOINTER ptr)
        {
            _Data = MetaTypes.GetData<CExtensionDefExplosionEffect>(meta, ptr);
            explosionName = MetaTypes.GetString(meta, _Data.explosionName);
        }

        public override MetaPOINTER Save(MetaBuilder mb)
        {
            if (explosionName != null)
            {
                _Data.explosionName = mb.AddStringPtr(explosionName);
            }
            mb.AddStructureInfo(MetaName.CExtensionDefExplosionEffect);
            return mb.AddItemPtr(MetaName.CExtensionDefExplosionEffect, _Data);
        }

        public override string Name
        {
            get
            {
                return _Data.name.ToString();
            }
        }

        public override string ToString()
        {
            return _Data.ToString() + " - " + explosionName;
        }
    }

    [TC(typeof(EXP))] public struct CExtensionDefLadder //96 bytes, Key:1978210597
    {
        public uint Unused0 { get; set; }//0
        public uint Unused1 { get; set; }//4
        public MetaHash name { get; set; } //8   8: Hash: 0: name
        public uint Unused2 { get; set; }//12
        public Vector3 offsetPosition { get; set; } //16   16: Float_XYZ: 0: offsetPosition
        public float Unused3 { get; set; }//28
        public Vector3 bottom { get; set; } //32   32: Float_XYZ: 0: bottom
        public float Unused4 { get; set; }//44
        public Vector3 top { get; set; } //48   48: Float_XYZ: 0: top
        public float Unused5 { get; set; }//60
        public Vector3 normal { get; set; } //64   64: Float_XYZ: 0: normal
        public float Unused6 { get; set; }//76
        public Unk_1294270217 materialType { get; set; } //80   80: IntEnum: 1294270217: materialType//932754174
        public MetaHash template { get; set; } //84   84: Hash: 0: template
        public byte canGetOffAtTop { get; set; } //88   88: Boolean: 0: canGetOffAtTop//564839673
        public byte canGetOffAtBottom { get; set; } //89   89: Boolean: 0: canGetOffAtBottom//923729576
        public ushort Unused7 { get; set; }//90
        public uint Unused8 { get; set; }//92

        public override string ToString()
        {
            return "CExtensionDefLadder - " + name.ToString();
        }
    }
    [TC(typeof(EXP))] public class MCExtensionDefLadder : MetaWrapper
    {
        public CExtensionDefLadder _Data;
        public CExtensionDefLadder Data { get { return _Data; } }

        public override void Load(Meta meta, MetaPOINTER ptr)
        {
            _Data = MetaTypes.GetData<CExtensionDefLadder>(meta, ptr);
        }

        public override MetaPOINTER Save(MetaBuilder mb)
        {
            mb.AddEnumInfo((MetaName)1294270217);
            mb.AddStructureInfo(MetaName.CExtensionDefLadder);
            return mb.AddItemPtr(MetaName.CExtensionDefLadder, _Data);
        }

        public override string Name
        {
            get
            {
                return _Data.name.ToString();
            }
        }

        public override string ToString()
        {
            return _Data.ToString();
        }
    }

    [TC(typeof(EXP))] public struct CExtensionDefBuoyancy //32 bytes, Key:2383039928
    {
        public uint Unused0 { get; set; }//0
        public uint Unused1 { get; set; }//4
        public MetaHash name { get; set; } //8   8: Hash: 0: name
        public uint Unused2 { get; set; }//12
        public Vector3 offsetPosition { get; set; } //16   16: Float_XYZ: 0: offsetPosition
        public float Unused3 { get; set; }//28

        public override string ToString()
        {
            return "CExtensionDefBuoyancy - " + name.ToString();
        }
    }
    [TC(typeof(EXP))] public class MCExtensionDefBuoyancy : MetaWrapper
    {
        public CExtensionDefBuoyancy _Data;
        public CExtensionDefBuoyancy Data { get { return _Data; } }

        public override void Load(Meta meta, MetaPOINTER ptr)
        {
            _Data = MetaTypes.GetData<CExtensionDefBuoyancy>(meta, ptr);
        }

        public override MetaPOINTER Save(MetaBuilder mb)
        {
            mb.AddStructureInfo(MetaName.CExtensionDefBuoyancy);
            return mb.AddItemPtr(MetaName.CExtensionDefBuoyancy, _Data);
        }

        public override string Name
        {
            get
            {
                return _Data.name.ToString();
            }
        }

        public override string ToString()
        {
            return _Data.ToString();
        }
    }

    [TC(typeof(EXP))] public struct CExtensionDefExpression //48 bytes, Key:24441706
    {
        public uint Unused0 { get; set; }//0
        public uint Unused1 { get; set; }//4
        public MetaHash name { get; set; } //8   8: Hash: 0: name
        public uint Unused2 { get; set; }//12
        public Vector3 offsetPosition { get; set; } //16   16: Float_XYZ: 0: offsetPosition
        public float Unused3 { get; set; }//28
        public MetaHash Unk_1095612811 { get; set; } //32   32: Hash: 0: 1095612811
        public MetaHash expressionName { get; set; } //36   36: Hash: 0: expressionName
        public MetaHash Unk_2766477159 { get; set; } //40   40: Hash: 0: 2766477159
        public byte Unk_1562817888 { get; set; } //44   44: Boolean: 0: 1562817888
        public byte Unused4 { get; set; }//45
        public ushort Unused5 { get; set; }//46

        public override string ToString()
        {
            return "CExtensionDefExpression - " + name.ToString();
        }
    }
    [TC(typeof(EXP))] public class MCExtensionDefExpression : MetaWrapper
    {
        public CExtensionDefExpression _Data;
        public CExtensionDefExpression Data { get { return _Data; } }

        public override void Load(Meta meta, MetaPOINTER ptr)
        {
            _Data = MetaTypes.GetData<CExtensionDefExpression>(meta, ptr);
        }

        public override MetaPOINTER Save(MetaBuilder mb)
        {
            mb.AddStructureInfo(MetaName.CExtensionDefExpression);
            return mb.AddItemPtr(MetaName.CExtensionDefExpression, _Data);
        }

        public override string Name
        {
            get
            {
                return _Data.name.ToString();
            }
        }

        public override string ToString()
        {
            return _Data.ToString();
        }
    }

    [TC(typeof(EXP))] public struct CExtensionDefLightShaft //176 bytes, Key:2526429398
    {
        public uint Unused00 { get; set; }//0
        public uint Unused01 { get; set; }//4
        public MetaHash name { get; set; } //8   8: Hash: 0: name
        public uint Unused02 { get; set; }//12
        public Vector3 offsetPosition { get; set; } //16   16: Float_XYZ: 0: offsetPosition
        public float Unused03 { get; set; }//28
        public Vector3 cornerA { get; set; } //32   32: Float_XYZ: 0: 3302595027
        public float Unused04 { get; set; }//44
        public Vector3 cornerB { get; set; } //48   48: Float_XYZ: 0: 2393877884
        public float Unused05 { get; set; }//60
        public Vector3 cornerC { get; set; } //64   64: Float_XYZ: 0: 2692731164
        public float Unused06 { get; set; }//76
        public Vector3 cornerD { get; set; } //80   80: Float_XYZ: 0: 4250372814
        public float Unused07 { get; set; }//92
        public Vector3 direction { get; set; } //96   96: Float_XYZ: 0: direction
        public float Unused08 { get; set; }//108
        public float directionAmount { get; set; } //112   112: Float: 0: 1441249296
        public float length { get; set; } //116   116: Float: 0: length
        public float Unk_1616789093 { get; set; } //120   120: Float: 0: 1616789093
        public float Unk_120454521 { get; set; } //124   124: Float: 0: 120454521
        public float Unk_1297365553 { get; set; } //128   128: Float: 0: 1297365553
        public float Unk_75548206 { get; set; } //132   132: Float: 0: 75548206
        public float Unk_40301253 { get; set; } //136   136: Float: 0: 40301253
        public float Unk_475013030 { get; set; } //140   140: Float: 0: 475013030
        public uint color { get; set; } //144   144: UnsignedInt: 0: color
        public float intensity { get; set; } //148   148: Float: 0: intensity
        public byte flashiness { get; set; } //152   152: UnsignedByte: 0: 3829693202
        public byte Unused09 { get; set; }//153
        public ushort Unused10 { get; set; }//154
        public uint flags { get; set; } //156   156: UnsignedInt: 0: flags
        public Unk_1931949281 densityType { get; set; } //160   160: IntEnum: 1931949281: densityType//235100599
        public Unk_2266515059 volumeType { get; set; } //164   164: IntEnum: 2266515059: volumeType//4021175589
        public float softness { get; set; } //168   168: Float: 0: softness//187712958
        public byte Unk_59101696 { get; set; } //172   172: Boolean: 0: 59101696
        public byte Unused11 { get; set; }//173
        public ushort Unused12 { get; set; }//174

        public override string ToString()
        {
            return "CExtensionDefLightShaft - " + name.ToString();
        }
    }
    [TC(typeof(EXP))] public class MCExtensionDefLightShaft : MetaWrapper
    {
        public CExtensionDefLightShaft _Data;
        public CExtensionDefLightShaft Data { get { return _Data; } }

        public override void Load(Meta meta, MetaPOINTER ptr)
        {
            _Data = MetaTypes.GetData<CExtensionDefLightShaft>(meta, ptr);
        }

        public override MetaPOINTER Save(MetaBuilder mb)
        {
            mb.AddEnumInfo((MetaName)1931949281);
            mb.AddEnumInfo((MetaName)2266515059);
            mb.AddStructureInfo(MetaName.CExtensionDefLightShaft);
            return mb.AddItemPtr(MetaName.CExtensionDefLightShaft, _Data);
        }

        public override string Name
        {
            get
            {
                return _Data.name.ToString();
            }
        }

        public override string ToString()
        {
            return _Data.ToString();
        }
    }

    [TC(typeof(EXP))] public struct CExtensionDefDoor //48 bytes, Key:2671601385
    {
        public uint Unused0 { get; set; }//0
        public uint Unused1 { get; set; }//4
        public MetaHash name { get; set; } //8   8: Hash: 0: name
        public uint Unused2 { get; set; }//12
        public Vector3 offsetPosition { get; set; } //16   16: Float_XYZ: 0: offsetPosition
        public float Unused3 { get; set; }//28
        public byte enableLimitAngle { get; set; } //32   32: Boolean: 0: enableLimitAngle=1979299226
        public byte startsLocked { get; set; } //33   33: Boolean: 0: startsLocked=3204572347
        public byte canBreak { get; set; } //34   34: Boolean: 0: canBreak=2756786344
        public byte Unused4 { get; set; }//35
        public float limitAngle { get; set; } //36   36: Float: 0: limitAngle
        public float doorTargetRatio { get; set; } //40   40: Float: 0: doorTargetRatio=770433283
        public MetaHash audioHash { get; set; } //44   44: Hash: 0: audioHash=224069936

        public override string ToString()
        {
            return "CExtensionDefDoor - " + name.ToString() + ", " + audioHash.ToString() + ", " + offsetPosition.ToString() + ", " + limitAngle.ToString();
        }
    }
    [TC(typeof(EXP))] public class MCExtensionDefDoor : MetaWrapper
    {
        public CExtensionDefDoor _Data;
        public CExtensionDefDoor Data { get { return _Data; } }

        public override void Load(Meta meta, MetaPOINTER ptr)
        {
            _Data = MetaTypes.GetData<CExtensionDefDoor>(meta, ptr);
        }

        public override MetaPOINTER Save(MetaBuilder mb)
        {
            mb.AddStructureInfo(MetaName.CExtensionDefDoor);
            return mb.AddItemPtr(MetaName.CExtensionDefDoor, _Data);
        }

        public override string Name
        {
            get
            {
                return _Data.name.ToString();
            }
        }

        public override string ToString()
        {
            return _Data.ToString();
        }
    }

    [TC(typeof(EXP))] public struct CExtensionDefSpawnPoint //96 bytes, Key:3077340721
    {
        public uint Unused0 { get; set; }//0
        public uint Unused1 { get; set; }//4
        public MetaHash name { get; set; } //8   8: Hash: 0: name
        public uint Unused2 { get; set; }//12
        public Vector3 offsetPosition { get; set; } //16   16: Float_XYZ: 0: offsetPosition
        public float Unused3 { get; set; }//28
        public Vector4 offsetRotation { get; set; } //32   32: Float_XYZW: 0: offsetRotation
        public MetaHash spawnType { get; set; } //48   48: Hash: 0: spawnType
        public MetaHash pedType { get; set; } //52   52: Hash: 0: pedType
        public MetaHash group { get; set; } //56   56: Hash: 0: group
        public MetaHash interior { get; set; } //60   60: Hash: 0: interior
        public MetaHash requiredImap { get; set; } //64   64: Hash: 0: requiredImap
        public Unk_3573596290 availableInMpSp { get; set; } //68   68: IntEnum: 3573596290: availableInMpSp
        public float probability { get; set; } //72   72: Float: 0: probability
        public float timeTillPedLeaves { get; set; } //76   76: Float: 0: timeTillPedLeaves
        public float radius { get; set; } //80   80: Float: 0: radius
        public byte start { get; set; } //84   84: UnsignedByte: 0: start
        public byte end { get; set; } //85   85: UnsignedByte: 0: end
        public ushort Unused4 { get; set; }//86
        public Unk_700327466 flags { get; set; } //88   88: IntFlags2: 700327466: flags
        public byte highPri { get; set; } //92   92: Boolean: 0: highPri
        public byte extendedRange { get; set; } //93   93: Boolean: 0: extendedRange
        public byte shortRange { get; set; } //94   94: Boolean: 0: shortRange
        public byte Unused5 { get; set; }//95

        public override string ToString()
        {
            return spawnType.ToString() + ": " + name.ToString() + ", " + pedType.ToString();// + ", " + flags.ToString() + ", " + offsetPosition.ToString();
        }
    }
    [TC(typeof(EXP))] public class MCExtensionDefSpawnPoint : MetaWrapper
    {
        [TC(typeof(EXP))] public object Parent { get; set; }
        public MCScenarioPointRegion ScenarioRegion { get; private set; }

        public CExtensionDefSpawnPoint _Data;
        public CExtensionDefSpawnPoint Data { get { return _Data; } }

        public Vector3 ParentPosition { get; set; } = Vector3.Zero;
        public Vector3 OffsetPosition { get { return _Data.offsetPosition; } set { _Data.offsetPosition = value; } }
        public Vector4 OffsetRotation { get { return _Data.offsetRotation; } set { _Data.offsetRotation = value; } }
        public MetaHash NameHash { get { return _Data.name; } set { _Data.name = value; } }
        public MetaHash SpawnType { get { return _Data.spawnType; } set { _Data.spawnType = value; } }
        public MetaHash PedType { get { return _Data.pedType; } set { _Data.pedType = value; } }
        public MetaHash Group { get { return _Data.group; } set { _Data.group = value; } }
        public MetaHash Interior { get { return _Data.interior; } set { _Data.interior = value; } }
        public MetaHash RequiredImap { get { return _Data.requiredImap; } set { _Data.requiredImap = value; } }
        public Unk_3573596290 AvailableInMpSp { get { return _Data.availableInMpSp; } set { _Data.availableInMpSp = value; } }
        public float Probability { get { return _Data.probability; } set { _Data.probability = value; } }
        public float TimeTillPedLeaves { get { return _Data.timeTillPedLeaves; } set { _Data.timeTillPedLeaves = value; } }
        public float Radius { get { return _Data.radius; } set { _Data.radius = value; } }
        public byte StartTime { get { return _Data.start; } set { _Data.start = value; } }
        public byte EndTime { get { return _Data.end; } set { _Data.end = value; } }
        public Unk_700327466 Flags { get { return _Data.flags; } set { _Data.flags = value; } }
        public bool HighPri { get { return _Data.highPri == 1; } set { _Data.highPri = (byte)(value ? 1 : 0); } }
        public bool ExtendedRange { get { return _Data.extendedRange == 1; } set { _Data.extendedRange = (byte)(value ? 1 : 0); } }
        public bool ShortRange { get { return _Data.shortRange == 1; } set { _Data.shortRange = (byte)(value ? 1 : 0); } }

        public Vector3 Position { get { return _Data.offsetPosition + ParentPosition; } set { _Data.offsetPosition = value - ParentPosition; } }
        public Quaternion Orientation { get { return new Quaternion(_Data.offsetRotation);  } set { _Data.offsetRotation = value.ToVector4(); } }

        public MCExtensionDefSpawnPoint() { }
        public MCExtensionDefSpawnPoint(MCScenarioPointRegion region, Meta meta, CExtensionDefSpawnPoint data, object parent)
        {
            ScenarioRegion = region;
            Parent = parent;
            _Data = data;
        }
        public MCExtensionDefSpawnPoint(MCScenarioPointRegion region, MCExtensionDefSpawnPoint copy)
        {
            ScenarioRegion = region;
            if (copy != null)
            {
                _Data = copy.Data;
                Parent = copy.Parent;
                ParentPosition = copy.ParentPosition;
            }
        }

        public override void Load(Meta meta, MetaPOINTER ptr)
        {
            _Data = MetaTypes.GetData<CExtensionDefSpawnPoint>(meta, ptr);
        }

        public override MetaPOINTER Save(MetaBuilder mb)
        {
            mb.AddEnumInfo((MetaName)3573596290);
            mb.AddEnumInfo((MetaName)700327466);
            mb.AddStructureInfo(MetaName.CExtensionDefSpawnPoint);
            return mb.AddItemPtr(MetaName.CExtensionDefSpawnPoint, _Data);
        }

        public override string Name
        {
            get
            {
                return _Data.name.ToString();
            }
        }

        public override string ToString()
        {
            return _Data.ToString();
        }
    }

    [TC(typeof(EXP))] public struct CExtensionDefSpawnPointOverride //64 bytes, Key:2551875873
    {
        public uint Unused0 { get; set; }//0
        public uint Unused1 { get; set; }//4
        public MetaHash name { get; set; } //8   8: Hash: 0: name
        public uint Unused2 { get; set; }//12
        public Vector3 offsetPosition { get; set; } //16   16: Float_XYZ: 0: offsetPosition
        public float Unused3 { get; set; }//28
        public MetaHash ScenarioType { get; set; } //32   32: Hash: 0: ScenarioType
        public byte iTimeStartOverride { get; set; } //36   36: UnsignedByte: 0: iTimeStartOverride//591476992
        public byte iTimeEndOverride { get; set; } //37   37: UnsignedByte: 0: iTimeEndOverride//2688038523
        public ushort Unused4 { get; set; }//38
        public MetaHash Group { get; set; } //40   40: Hash: 0: Group
        public MetaHash ModelSet { get; set; } //44   44: Hash: 0: ModelSet
        public Unk_3573596290 AvailabilityInMpSp { get; set; } //48   48: IntEnum: 3573596290: AvailabilityInMpSp//2932681318
        public Unk_700327466 Flags { get; set; } //52   52: IntFlags2: 700327466: Flags
        public float Radius { get; set; } //56   56: Float: 0: Radius
        public float TimeTillPedLeaves { get; set; } //60   60: Float: 0: TimeTillPedLeaves//4073598194

        public override string ToString()
        {
            return "CExtensionDefSpawnPointOverride - " + name.ToString();
        }
    }
    [TC(typeof(EXP))] public class MCExtensionDefSpawnPointOverride : MetaWrapper
    {
        public CExtensionDefSpawnPointOverride _Data;
        public CExtensionDefSpawnPointOverride Data { get { return _Data; } }

        public override void Load(Meta meta, MetaPOINTER ptr)
        {
            _Data = MetaTypes.GetData<CExtensionDefSpawnPointOverride>(meta, ptr);
        }

        public override MetaPOINTER Save(MetaBuilder mb)
        {
            mb.AddEnumInfo((MetaName)3573596290);
            mb.AddEnumInfo((MetaName)700327466);
            mb.AddStructureInfo(MetaName.CExtensionDefSpawnPointOverride);
            return mb.AddItemPtr(MetaName.CExtensionDefSpawnPointOverride, _Data);
        }

        public override string Name
        {
            get
            {
                return _Data.name.ToString();
            }
        }

        public override string ToString()
        {
            return _Data.ToString();
        }
    }

    [TC(typeof(EXP))] public struct CExtensionDefWindDisturbance //96 bytes, Key:3971538917
    {
        public uint Unused0 { get; set; }//0
        public uint Unused1 { get; set; }//4
        public MetaHash name { get; set; } //8   8: Hash: 0: name
        public uint Unused2 { get; set; }//12
        public Vector3 offsetPosition { get; set; } //16   16: Float_XYZ: 0: offsetPosition
        public float Unused3 { get; set; }//28
        public Vector4 offsetRotation { get; set; } //32   32: Float_XYZW: 0: offsetRotation
        public int disturbanceType { get; set; } //48   48: SignedInt: 0: disturbanceType//3802708370
        public int boneTag { get; set; } //52   52: SignedInt: 0: boneTag
        public uint Unused4 { get; set; }//56
        public uint Unused5 { get; set; }//60
        public Vector4 size { get; set; } //64   64: Float_XYZW: 0: size
        public float strength { get; set; } //80   80: Float: 0: strength
        public int flags { get; set; } //84   84: SignedInt: 0: flags
        public uint Unused6 { get; set; }//88
        public uint Unused7 { get; set; }//92

        public override string ToString()
        {
            return "CExtensionDefWindDisturbance - " + name.ToString();
        }
    }
    [TC(typeof(EXP))] public class MCExtensionDefWindDisturbance : MetaWrapper
    {
        public CExtensionDefWindDisturbance _Data;
        public CExtensionDefWindDisturbance Data { get { return _Data; } }

        public override void Load(Meta meta, MetaPOINTER ptr)
        {
            _Data = MetaTypes.GetData<CExtensionDefWindDisturbance>(meta, ptr);
        }

        public override MetaPOINTER Save(MetaBuilder mb)
        {
            mb.AddStructureInfo(MetaName.CExtensionDefWindDisturbance);
            return mb.AddItemPtr(MetaName.CExtensionDefWindDisturbance, _Data);
        }

        public override string Name
        {
            get
            {
                return _Data.name.ToString();
            }
        }

        public override string ToString()
        {
            return _Data.ToString();
        }
    }

    [TC(typeof(EXP))] public struct CExtensionDefProcObject //80 bytes, Key:3965391891
    {
        public uint Unused0 { get; set; }//0
        public uint Unused1 { get; set; }//4
        public MetaHash name { get; set; } //8   8: Hash: 0: name
        public uint Unused2 { get; set; }//12
        public Vector3 offsetPosition { get; set; } //16   16: Float_XYZ: 0: offsetPosition
        public float Unused3 { get; set; }//28
        public float radiusInner { get; set; } //32   32: Float: 0: radiusInner//406390660
        public float radiusOuter { get; set; } //36   36: Float: 0: radiusOuter//1814053978
        public float spacing { get; set; } //40   40: Float: 0: spacing
        public float minScale { get; set; } //44   44: Float: 0: minScale//3662913353
        public float maxScale { get; set; } //48   48: Float: 0: maxScale//803384552
        public float Unk_3913056845 { get; set; } //52   52: Float: 0: 3913056845
        public float Unk_147400493 { get; set; } //56   56: Float: 0: 147400493
        public float Unk_2591582364 { get; set; } //60   60: Float: 0: 2591582364
        public float Unk_3889902555 { get; set; } //64   64: Float: 0: 3889902555
        public uint objectHash { get; set; } //68   68: UnsignedInt: 0: objectHash//1951307499
        public uint flags { get; set; } //72   72: UnsignedInt: 0: flags
        public uint Unused4 { get; set; }//76

        public override string ToString()
        {
            return "CExtensionDefProcObject - " + name.ToString();
        }
    }
    [TC(typeof(EXP))] public class MCExtensionDefProcObject : MetaWrapper
    {
        public CExtensionDefProcObject _Data;
        public CExtensionDefProcObject Data { get { return _Data; } }

        public override void Load(Meta meta, MetaPOINTER ptr)
        {
            _Data = MetaTypes.GetData<CExtensionDefProcObject>(meta, ptr);
        }

        public override MetaPOINTER Save(MetaBuilder mb)
        {
            mb.AddStructureInfo(MetaName.CExtensionDefProcObject);
            return mb.AddItemPtr(MetaName.CExtensionDefProcObject, _Data);
        }

        public override string Name
        {
            get
            {
                return _Data.name.ToString();
            }
        }

        public override string ToString()
        {
            return _Data.ToString();
        }
    }

    [TC(typeof(EXP))] public struct rage__phVerletClothCustomBounds //32 bytes, Key:2075461750
    {
        public uint Unused0 { get; set; }//0
        public uint Unused1 { get; set; }//4
        public MetaHash name { get; set; } //8   8: Hash: 0: name
        public uint Unused2 { get; set; }//12
        public Array_Structure CollisionData { get; set; } //16   16: Array: 0: CollisionData  {0: Structure: SectionUNKNOWN1: 256}
    }
    [TC(typeof(EXP))] public class Mrage__phVerletClothCustomBounds : MetaWrapper
    {
        public rage__phVerletClothCustomBounds _Data;
        public rage__phVerletClothCustomBounds Data { get { return _Data; } }

        public MUnk_1701774085[] CollisionData { get; set; }

        public override void Load(Meta meta, MetaPOINTER ptr)
        {
            _Data = MetaTypes.GetData<rage__phVerletClothCustomBounds>(meta, ptr);

            var cdata = MetaTypes.ConvertDataArray<Unk_1701774085>(meta, (MetaName)1701774085/*.SectionUNKNOWN1*/, _Data.CollisionData);
            if (cdata != null)
            {
                CollisionData = new MUnk_1701774085[cdata.Length];
                for (int i = 0; i < cdata.Length; i++)
                {
                    CollisionData[i] = new MUnk_1701774085(meta, cdata[i]);
                }
            }
        }

        public override MetaPOINTER Save(MetaBuilder mb)
        {
            if (CollisionData != null)
            {
                _Data.CollisionData = mb.AddWrapperArray(CollisionData);
            }

            mb.AddStructureInfo(MetaName.rage__phVerletClothCustomBounds);
            return mb.AddItemPtr(MetaName.rage__phVerletClothCustomBounds, _Data);
        }

        public override string ToString()
        {
            return "rage__phVerletClothCustomBounds - " + _Data.name.ToString() + " (" + (CollisionData?.Length ?? 0).ToString() + " CollisionData)";
        }
    }

    [TC(typeof(EXP))] public struct Unk_1701774085 //96 bytes, Key:2859775340 //dexy: cloth CollisionData (child of rage__phVerletClothCustomBounds) ... eg josh house  // Tom: explosions? 
    {
        public CharPointer OwnerName { get; set; } //0   0: CharPointer: 0: OwnerName
        public Vector4 Rotation { get; set; } //16   16: Float_XYZW: 0: Rotation
        public Vector3 Position { get; set; } //32   32: Float_XYZ: 0: Position
        public float Unused0 { get; set; }//44
        public Vector3 Normal { get; set; } //48   48: Float_XYZ: 0: Normal
        public float Unused1 { get; set; }//60
        public float CapsuleRadius { get; set; } //64   64: Float: 0: CapsuleRadius
        public float CapsuleLen { get; set; } //68   68: Float: 0: CapsuleLen
        public float CapsuleHalfHeight { get; set; } //72   72: Float: 0: CapsuleHalfHeight
        public float CapsuleHalfWidth { get; set; } //76   76: Float: 0: CapsuleHalfWidth
        public Unk_3044470860 Flags { get; set; } //80   80: IntFlags2: 3044470860: Flags
        public uint Unused2 { get; set; }//84
        public uint Unused3 { get; set; }//88
        public uint Unused4 { get; set; }//92
    }
    [TC(typeof(EXP))] public class MUnk_1701774085 : MetaWrapper
    {
        public Unk_1701774085 _Data;
        public Unk_1701774085 Data { get { return _Data; } }

        public string OwnerName { get; set; }

        public MUnk_1701774085() { }
        public MUnk_1701774085(Meta meta, Unk_1701774085 s)
        {
            _Data = s;
            OwnerName = MetaTypes.GetString(meta, _Data.OwnerName);
        }
        public override void Load(Meta meta, MetaPOINTER ptr)
        {
            _Data = MetaTypes.GetData<Unk_1701774085>(meta, ptr);
            OwnerName = MetaTypes.GetString(meta, _Data.OwnerName);
        }

        public override MetaPOINTER Save(MetaBuilder mb)
        {
            if (OwnerName != null)
            {
                _Data.OwnerName = mb.AddStringPtr(OwnerName);
            }

            mb.AddEnumInfo((MetaName)3044470860);
            mb.AddStructureInfo((MetaName)1701774085/*.SectionUNKNOWN1*/);
            return mb.AddItemPtr((MetaName)1701774085/*.SectionUNKNOWN1*/, _Data);
        }

        public override string ToString()
        {
            return "Unk_1701774085 - " + OwnerName;
        }
    }










    [TC(typeof(EXP))] public struct rage__spdGrid2D //64 bytes, Key:894636096
    {
        public uint Unused0 { get; set; }//0
        public uint Unused1 { get; set; }//4
        public uint Unused2 { get; set; }//8
        public int Unk_MinX_860552138 { get; set; } //12   12: SignedInt: 0: 860552138   //MIN X
        public int Unk_MaxX_3824598937 { get; set; } //16   16: SignedInt: 0: 3824598937 //MAX X
        public int Unk_MinY_496029782 { get; set; } //20   20: SignedInt: 0: 496029782   //MIN Y
        public int Unk_MaxY_3374647798 { get; set; } //24   24: SignedInt: 0: 3374647798 //MAX Y
        public uint Unused3 { get; set; }//28
        public uint Unused4 { get; set; }//32
        public uint Unused5 { get; set; }//36
        public uint Unused6 { get; set; }//40
        public float Unk_X_2690909759 { get; set; } //44   44: Float: 0: 2690909759 //grid scale X (cell size)
        public float Unk_Y_3691675019 { get; set; } //48   48: Float: 0: 3691675019 //grid scale Y (cell size)
        public uint Unused7 { get; set; }//52
        public uint Unused8 { get; set; }//56
        public uint Unused9 { get; set; }//60


        public Vector2I Dimensions
        {
            get
            {
                return new Vector2I((Unk_MaxX_3824598937 - Unk_MinX_860552138)+1, (Unk_MaxY_3374647798 - Unk_MinY_496029782)+1);
            }
        }
        public Vector2 Scale
        {
            get
            {
                return new Vector2(Unk_X_2690909759, Unk_Y_3691675019);
            }
            set
            {
                Unk_X_2690909759 = value.X;
                Unk_Y_3691675019 = value.Y;
            }
        }
        public Vector2 Min
        {
            get
            {
                return new Vector2(Unk_MinX_860552138, Unk_MinY_496029782) * Scale;
            }
            set
            {
                var gv = value / Scale;
                Unk_MinX_860552138 = (int)Math.Floor(gv.X);
                Unk_MinY_496029782 = (int)Math.Floor(gv.Y);
            }
        }
        public Vector2 Max
        {
            get
            {
                return new Vector2(Unk_MaxX_3824598937, Unk_MaxY_3374647798) * Scale;
            }
            set
            {
                var gv = value / Scale;
                Unk_MaxX_3824598937 = (int)Math.Floor(gv.X);
                Unk_MaxY_3374647798 = (int)Math.Floor(gv.Y);
            }
        }
    }

    [TC(typeof(EXP))] public struct rage__spdAABB //32 bytes, Key:1158138379  //WAS: Unk_4084721864
    {
        public Vector4 min { get; set; } //0   0: Float_XYZW: 0: min
        public Vector4 max { get; set; } //16   16: Float_XYZW: 0: max

        public override string ToString()
        {
            return "min: " + min.ToString() + ", max: " + max.ToString();
        }
        public void SwapEnd()
        {
            min = MetaTypes.SwapBytes(min);
            max = MetaTypes.SwapBytes(max);
        }
    }

    [TC(typeof(EXP))] public struct rage__spdSphere //16 bytes, Key:1189037266  //Sphere - used in scenario parts 
    {
        public Vector4 centerAndRadius { get; set; } //0   0: Float_XYZW: 0: centerAndRadius

        public override string ToString()
        {
            return centerAndRadius.ToString();
        }
    }





    [TC(typeof(EXP))] public struct CScenarioPointRegion  //SCENARIO YMT ROOT  - in /scenario/ folder //376 bytes, Key:3501351821
    {
        public int VersionNumber { get; set; } //0   0: SignedInt: 0: VersionNumber
        public uint Unused0 { get; set; }//4
        public CScenarioPointContainer Points { get; set; } //8   8: Structure: CScenarioPointContainer//2380938603: Points//702683191
        public uint Unused1 { get; set; }//56
        public uint Unused2 { get; set; }//60
        public uint Unused3 { get; set; }//64
        public uint Unused4 { get; set; }//68
        public Array_Structure EntityOverrides { get; set; } //72   72: Array: 0: EntityOverrides//697469539  {0: Structure: CScenarioEntityOverride//4213733800: 256}
        public uint Unused5 { get; set; }//88
        public uint Unused6 { get; set; }//92
        public Unk_4023740759 Unk_3696045377 { get; set; } //[PATHS] 96   96: Structure: 4023740759: 3696045377
        public rage__spdGrid2D AccelGrid { get; set; } //184   184: Structure: rage__spdGrid2D: AccelGrid//3053155275
        public Array_ushort Unk_3844724227 { get; set; } //248   248: Array: 0: 3844724227  {0: UnsignedShort: 0: 256}
        public Array_Structure Clusters { get; set; } //264   264: Array: 0: Clusters//3587988394  {0: Structure: CScenarioPointCluster//750308016: 256}
        public CScenarioPointLookUps LookUps { get; set; } //280   280: Structure: CScenarioPointLookUps//3019621867: LookUps//1097626284
    }
    [TC(typeof(EXP))] public class MCScenarioPointRegion : MetaWrapper
    {
        public YmtFile Ymt { get; set; }

        public CScenarioPointRegion _Data;
        public CScenarioPointRegion Data { get { return _Data; } }

        public MCScenarioPointContainer Points { get; set; }
        public MCScenarioEntityOverride[] EntityOverrides { get; set; }
        public MUnk_4023740759 Paths { get; set; }
        public ushort[] Unk_3844724227 { get; set; } //GRID DATA - 2d dimensions - AccelGrid ((MaxX-MinX)+1)*((MaxY-MinY)+1)
        public MCScenarioPointCluster[] Clusters { get; set; }
        public MCScenarioPointLookUps LookUps { get; set; }

        public int VersionNumber { get { return _Data.VersionNumber; } set { _Data.VersionNumber = value; } }

        public override void Load(Meta meta, MetaPOINTER ptr)
        {
            var data = MetaTypes.GetData<CScenarioPointRegion>(meta, ptr);
            Load(meta, data);
        }
        public void Load(Meta meta, CScenarioPointRegion data)
        {
            _Data = data;


            Points = new MCScenarioPointContainer(this, meta, _Data.Points);


            var entOverrides = MetaTypes.ConvertDataArray<CScenarioEntityOverride>(meta, MetaName.CScenarioEntityOverride, _Data.EntityOverrides);
            if (entOverrides != null)
            {
                EntityOverrides = new MCScenarioEntityOverride[entOverrides.Length];
                for (int i = 0; i < entOverrides.Length; i++)
                {
                    EntityOverrides[i] = new MCScenarioEntityOverride(this, meta, entOverrides[i]);
                }
            }


            Paths = new MUnk_4023740759(this, meta, _Data.Unk_3696045377);


            var clusters = MetaTypes.ConvertDataArray<CScenarioPointCluster>(meta, MetaName.CScenarioPointCluster, _Data.Clusters);
            if (clusters != null)
            {
                Clusters = new MCScenarioPointCluster[clusters.Length];
                for (int i = 0; i < clusters.Length; i++)
                {
                    Clusters[i] = new MCScenarioPointCluster(this, meta, clusters[i]);
                }
            }

            Unk_3844724227 = MetaTypes.GetUshortArray(meta, _Data.Unk_3844724227);

            LookUps = new MCScenarioPointLookUps(this, meta, _Data.LookUps);


            #region data analysis
            ////data analysis
            //if (Points.LoadSavePoints != null)
            //{ } //no hits here!
            //if (Unk_3844724227 != null)
            //{
            //    var grid = _Data.AccelGrid;
            //    var minx = grid.Unk_MinX_860552138;
            //    var maxx = grid.Unk_MaxX_3824598937;
            //    var miny = grid.Unk_MinY_496029782;
            //    var maxy = grid.Unk_MaxY_3374647798;
            //    var len = Unk_3844724227.Length;
            //    var calclen = ((maxx - minx) + 1) * ((maxy - miny) + 1);
            //    if (len != calclen)
            //    { } //no hits here!
            //    int pointcount = 0;
            //    if (Points.MyPoints != null) pointcount += Points.MyPoints.Length;
            //    //if (Points.LoadSavePoints != null) pointcount += Points.LoadSavePoints.Length;//not necessary!
            //    int lastuval = 0;
            //    for (int i = 0; i < Unk_3844724227.Length; i++)
            //    {
            //        var uval = Unk_3844724227[i];
            //        var uval2 = uval & 0x7FFF;
            //        var uval3 = uval >> 15; //what does this bit mean?
            //        if (uval3 > 0)
            //        { }
            //        lastuval = uval2;
            //        if (uval2 > pointcount)
            //        { } //no hits here!
            //    }
            //    if (lastuval != pointcount)
            //    { } //no hits here!
            //}
            #endregion
        }

        public override MetaPOINTER Save(MetaBuilder mb)
        {

            var sprb = mb.EnsureBlock(MetaName.CScenarioPointRegion);
            //var hashb = mb.EnsureBlock(MetaName.HASH);
            //var ushb = mb.EnsureBlock(MetaName.USHORT);
            //var pntb = mb.EnsureBlock(MetaName.CScenarioPoint);

            mb.AddStructureInfo(MetaName.CScenarioPointContainer);
            mb.AddStructureInfo((MetaName)4023740759);
            mb.AddStructureInfo(MetaName.rage__spdGrid2D);
            mb.AddStructureInfo(MetaName.CScenarioPointLookUps);
            mb.AddStructureInfo(MetaName.CScenarioPointRegion);


            if (Points != null)
            {
                var scp = new CScenarioPointContainer();

                var loadSavePoints = Points.GetCLoadSavePoints();
                if (loadSavePoints != null)//this never seems to be used...
                {
                    mb.AddStructureInfo(MetaName.CExtensionDefSpawnPoint);
                    mb.AddEnumInfo((MetaName)3573596290);
                    mb.AddEnumInfo((MetaName)700327466);
                    scp.LoadSavePoints = mb.AddItemArrayPtr(MetaName.CExtensionDefSpawnPoint, loadSavePoints);
                }
                var myPoints = Points.GetCMyPoints();
                if (myPoints != null)
                {
                    mb.AddStructureInfo(MetaName.CScenarioPoint);
                    mb.AddEnumInfo((MetaName)700327466);
                    scp.MyPoints = mb.AddItemArrayPtr(MetaName.CScenarioPoint, myPoints);
                }

                _Data.Points = scp;
            }
            else
            {
                _Data.Points = new CScenarioPointContainer();
            }


            if ((EntityOverrides != null) && (EntityOverrides.Length > 0))
            {
                //_Data.EntityOverrides = mb.AddWrapperArray(EntityOverrides);


                mb.AddStructureInfo(MetaName.CScenarioEntityOverride);
                var cents = new CScenarioEntityOverride[EntityOverrides.Length];
                for (int i = 0; i < EntityOverrides.Length; i++)
                {
                    var mcent = EntityOverrides[i];
                    var cent = mcent.Data;
                    var scps = mcent.GetCScenarioPoints();
                    if (scps != null)
                    {
                        mb.AddStructureInfo(MetaName.CExtensionDefSpawnPoint);
                        mb.AddEnumInfo((MetaName)3573596290);
                        mb.AddEnumInfo((MetaName)700327466);
                        cent.ScenarioPoints = mb.AddItemArrayPtr(MetaName.CExtensionDefSpawnPoint, scps);
                    }
                    cents[i] = cent;
                }
                _Data.EntityOverrides = mb.AddItemArrayPtr(MetaName.CScenarioEntityOverride, cents);

            }
            else
            {
                _Data.EntityOverrides = new Array_Structure();
            }


            if (Paths != null)
            {
                var pd = new Unk_4023740759();

                var nodes = Paths.GetCNodes();
                if (nodes != null)
                {
                    mb.AddStructureInfo(MetaName.CScenarioChainingNode);
                    pd.Nodes = mb.AddItemArrayPtr(MetaName.CScenarioChainingNode, nodes);
                }
                var edges = Paths.GetCEdges();
                if (edges != null)
                {
                    mb.AddStructureInfo(MetaName.CScenarioChainingEdge);
                    mb.AddEnumInfo((MetaName)3609807418);
                    mb.AddEnumInfo((MetaName)3971773454);
                    mb.AddEnumInfo((MetaName)941086046);
                    pd.Edges = mb.AddItemArrayPtr(MetaName.CScenarioChainingEdge, edges);
                }
                if (Paths.Chains != null)
                {
                    foreach (var chain in Paths.Chains)
                    {
                        if (chain.EdgeIds != null)
                        {
                            chain._Data.EdgeIds = mb.AddUshortArrayPtr(chain.EdgeIds);
                        }
                        else
                        {
                            chain._Data.EdgeIds = new Array_ushort();
                        }
                    }
                }
                var chains = Paths.GetCChains();
                if (chains != null)
                {
                    mb.AddStructureInfo(MetaName.CScenarioChain);
                    pd.Chains = mb.AddItemArrayPtr(MetaName.CScenarioChain, chains);
                }

                _Data.Unk_3696045377 = pd;
            }
            else
            {
                _Data.Unk_3696045377 = new Unk_4023740759();
            }


            if ((Clusters != null) && (Clusters.Length > 0))
            {
                //mb.AddStructureInfo(MetaName.rage__spdSphere);
                //mb.AddStructureInfo(MetaName.CScenarioPointCluster);
                _Data.Clusters = mb.AddWrapperArray(Clusters);
            }
            else
            {
                _Data.Clusters = new Array_Structure();
            }


            if ((Unk_3844724227 != null) && (Unk_3844724227.Length > 0))
            {
                _Data.Unk_3844724227 = mb.AddUshortArrayPtr(Unk_3844724227);
            }
            else
            {
                _Data.Unk_3844724227 = new Array_ushort();
            }


            if (LookUps != null)
            {
                var spl = new CScenarioPointLookUps();
                if ((LookUps.TypeNames != null) && (LookUps.TypeNames.Length > 0))
                {
                    spl.TypeNames = mb.AddHashArrayPtr(LookUps.TypeNames);
                }
                if ((LookUps.PedModelSetNames != null) && (LookUps.PedModelSetNames.Length > 0))
                {
                    spl.PedModelSetNames = mb.AddHashArrayPtr(LookUps.PedModelSetNames);
                }
                if ((LookUps.VehicleModelSetNames != null) && (LookUps.VehicleModelSetNames.Length > 0))
                {
                    spl.VehicleModelSetNames = mb.AddHashArrayPtr(LookUps.VehicleModelSetNames);
                }
                if ((LookUps.GroupNames != null) && (LookUps.GroupNames.Length > 0))
                {
                    spl.GroupNames = mb.AddHashArrayPtr(LookUps.GroupNames);
                }
                if ((LookUps.InteriorNames != null) && (LookUps.InteriorNames.Length > 0))
                {
                    spl.InteriorNames = mb.AddHashArrayPtr(LookUps.InteriorNames);
                }
                if ((LookUps.RequiredIMapNames != null) && (LookUps.RequiredIMapNames.Length > 0))
                {
                    spl.RequiredIMapNames = mb.AddHashArrayPtr(LookUps.RequiredIMapNames);
                }
                _Data.LookUps = spl;
            }
            else
            {
                _Data.LookUps = new CScenarioPointLookUps(); //this shouldn't happen...
            }

            mb.AddString("Made with CodeWalker by dexyfex. " + DateTime.Now.ToString());

            return mb.AddItemPtr(MetaName.CScenarioPointRegion, _Data);
        }



        public void AddCluster(MCScenarioPointCluster cluster)
        {
            List<MCScenarioPointCluster> newclusters = new List<MCScenarioPointCluster>();
            if (Clusters != null)
            {
                newclusters.AddRange(Clusters);
            }
            cluster.Region = this;
            //cluster.ClusterIndex = newclusters.Count;
            newclusters.Add(cluster);
            Clusters = newclusters.ToArray();
        }
        public void AddEntity(MCScenarioEntityOverride ent)
        {
            List<MCScenarioEntityOverride> newents = new List<MCScenarioEntityOverride>();
            if (EntityOverrides != null)
            {
                newents.AddRange(EntityOverrides);
            }
            ent.Region = this;
            //ent.EntityIndex = newents.Count;
            newents.Add(ent);
            EntityOverrides = newents.ToArray();
        }

        public bool RemoveCluster(MCScenarioPointCluster cluster)
        {
            bool r = false;
            if (Clusters != null)
            {
                List<MCScenarioPointCluster> newclusters = new List<MCScenarioPointCluster>();
                foreach (var nc in Clusters)
                {
                    if (nc == cluster)
                    {
                        r = true;
                    }
                    else
                    {
                        //nc.ClusterIndex = newclusters.Count;
                        newclusters.Add(nc);
                    }
                }
                if (r)
                {
                    Clusters = newclusters.ToArray();
                }
            }
            return r;
        }
        public bool RemoveEntity(MCScenarioEntityOverride ent)
        {
            bool r = false;
            if (EntityOverrides != null)
            {
                List<MCScenarioEntityOverride> newents = new List<MCScenarioEntityOverride>();
                foreach (var nc in EntityOverrides)
                {
                    if (nc == ent)
                    {
                        r = true;
                    }
                    else
                    {
                        //nc.EntityIndex = newents.Count;
                        newents.Add(nc);
                    }
                }
                if (r)
                {
                    EntityOverrides = newents.ToArray();
                }
            }
            return r;
        }



        public override string Name
        {
            get
            {
                return Ymt?.ToString() ?? "CScenarioPointRegion";
            }
        }

        public override string ToString()
        {
            return Name;
        }

    }

    [TC(typeof(EXP))] public struct CScenarioPointContainer  //SCENARIO Region Points arrays // 48 bytes, Key:2489654897
    {
        public Array_Structure LoadSavePoints { get; set; } //0   0: Array: 0: LoadSavePoints//3016741991  {0: Structure: CExtensionDefSpawnPoint: 256}
        public Array_Structure MyPoints { get; set; } //16   16: Array: 0: MyPoints//1170781136  {0: Structure: CScenarioPoint//4103049490: 256}
        public uint Unused0 { get; set; }//32
        public uint Unused1 { get; set; }//36
        public uint Unused2 { get; set; }//40
        public uint Unused3 { get; set; }//44

        public override string ToString()
        {
            return LoadSavePoints.Count1.ToString() + " LoadSavePoints, " + MyPoints.Count1.ToString() + " MyPoints";
        }
    }
    [TC(typeof(EXP))] public class MCScenarioPointContainer : MetaWrapper
    {
        [TC(typeof(EXP))] public object Parent { get; set; }
        public MCScenarioPointRegion Region { get; private set; }

        public CScenarioPointContainer _Data;
        public CScenarioPointContainer Data { get { return _Data; } set { _Data = value; } }

        public MCExtensionDefSpawnPoint[] LoadSavePoints { get; set; }
        public MCScenarioPoint[] MyPoints { get; set; }



        public MCScenarioPointContainer() { }
        public MCScenarioPointContainer(MCScenarioPointRegion region)
        {
            Region = region;
        }
        public MCScenarioPointContainer(MCScenarioPointRegion region, Meta meta, CScenarioPointContainer d)
        {
            Region = region;
            _Data = d;
            Init(meta);
        }

        public void Init(Meta meta)
        {
            var vLoadSavePoints = MetaTypes.ConvertDataArray<CExtensionDefSpawnPoint>(meta, MetaName.CExtensionDefSpawnPoint, _Data.LoadSavePoints);
            if (vLoadSavePoints != null)
            {
                LoadSavePoints = new MCExtensionDefSpawnPoint[vLoadSavePoints.Length];
                for (int i = 0; i < vLoadSavePoints.Length; i++)
                {
                    LoadSavePoints[i] = new MCExtensionDefSpawnPoint(Region, meta, vLoadSavePoints[i], this);
                }
            }

            var vMyPoints = MetaTypes.ConvertDataArray<CScenarioPoint>(meta, MetaName.CScenarioPoint, _Data.MyPoints);
            if (vMyPoints != null)
            {
                MyPoints = new MCScenarioPoint[vMyPoints.Length];
                for (int i = 0; i < vMyPoints.Length; i++)
                {
                    MyPoints[i] = new MCScenarioPoint(Region, meta, vMyPoints[i], this);
                    MyPoints[i].PointIndex = i;
                }
            }
        }

        public override void Load(Meta meta, MetaPOINTER ptr)
        {
            _Data = MetaTypes.GetData<CScenarioPointContainer>(meta, ptr);
            Init(meta);
        }

        public override MetaPOINTER Save(MetaBuilder mb)
        {
            mb.AddStructureInfo(MetaName.CScenarioPointContainer);
            return mb.AddItemPtr(MetaName.CScenarioPointContainer, _Data);
        }


        public CExtensionDefSpawnPoint[] GetCLoadSavePoints()
        {
            if ((LoadSavePoints == null) || (LoadSavePoints.Length == 0)) return null;
            CExtensionDefSpawnPoint[] r = new CExtensionDefSpawnPoint[LoadSavePoints.Length];
            for (int i = 0; i < LoadSavePoints.Length; i++)
            {
                r[i] = LoadSavePoints[i].Data;
            }
            return r;
        }
        public CScenarioPoint[] GetCMyPoints()
        {
            if ((MyPoints == null) || (MyPoints.Length == 0)) return null;
            CScenarioPoint[] r = new CScenarioPoint[MyPoints.Length];
            for (int i = 0; i < MyPoints.Length; i++)
            {
                r[i] = MyPoints[i].Data;
            }
            return r;
        }

        public void AddMyPoint(MCScenarioPoint p)
        {
            List<MCScenarioPoint> newpoints = new List<MCScenarioPoint>();
            if (MyPoints != null)
            {
                newpoints.AddRange(MyPoints);
            }
            p.PointIndex = newpoints.Count;
            newpoints.Add(p);
            p.Container = this;
            MyPoints = newpoints.ToArray();
        }
        public void AddLoadSavePoint(MCExtensionDefSpawnPoint p)
        {
            List<MCExtensionDefSpawnPoint> newpoints = new List<MCExtensionDefSpawnPoint>();
            if (LoadSavePoints != null)
            {
                newpoints.AddRange(LoadSavePoints);
            }
            newpoints.Add(p);
            p.Parent = this;
            LoadSavePoints = newpoints.ToArray();
        }

        public bool RemoveMyPoint(MCScenarioPoint p)
        {
            bool r = false;
            if (MyPoints != null)
            {
                List<MCScenarioPoint> newpoints = new List<MCScenarioPoint>();
                foreach (var mp in MyPoints)
                {
                    if (mp == p)
                    {
                        r = true;
                    }
                    else
                    {
                        newpoints.Add(mp);
                    }
                }
                if (r)
                {
                    MyPoints = newpoints.ToArray();

                    for (int i = 0; i < MyPoints.Length; i++)
                    {
                        MyPoints[i].PointIndex = i;
                    }
                }
            }
            return r;
        }
        public bool RemoveLoadSavePoint(MCExtensionDefSpawnPoint p)
        {
            bool r = false;
            if (LoadSavePoints != null)
            {
                List<MCExtensionDefSpawnPoint> newpoints = new List<MCExtensionDefSpawnPoint>();
                foreach (var mp in LoadSavePoints)
                {
                    if (mp == p)
                    {
                        r = true;
                    }
                    else
                    {
                        newpoints.Add(mp);
                    }
                }
                if (r)
                {
                    LoadSavePoints = newpoints.ToArray();
                }
            }
            return r;
        }



        public override string Name
        {
            get
            {
                return "CScenarioPointContainer";
            }
        }

        public override string ToString()
        {
            return _Data.ToString();
        }

    }

    [TC(typeof(EXP))] public struct CScenarioPoint  //SCENARIO Point, similar to CExtensionDefSpawnPointOverride //64 bytes, Key:402442150
    {
        public uint Unused0 { get; set; }//0
        public uint Unused1 { get; set; }//4
        public uint Unused2 { get; set; }//8
        public uint Unused3 { get; set; }//12
        public uint Unused4 { get; set; }//16
        public byte Unused5 { get; set; }//20
        public byte iType { get; set; } //21   21: UnsignedByte: 0: iType
        public byte ModelSetId { get; set; } //22   22: UnsignedByte: 0: ModelSetId//3361647288
        public byte iInterior { get; set; } //23   23: UnsignedByte: 0: 1975994103
        public byte iRequiredIMapId { get; set; } //24   24: UnsignedByte: 0: iRequiredIMapId//1229525587
        public byte iProbability { get; set; } //25   25: UnsignedByte: 0: iProbability//2974610960
        public byte uAvailableInMpSp { get; set; } //26   26: UnsignedByte: 0: uAvailableInMpSp//717991212
        public byte iTimeStartOverride { get; set; } //27   27: UnsignedByte: 0: 591476992
        public byte iTimeEndOverride { get; set; } //28   28: UnsignedByte: 0: 2688038523
        public byte iRadius { get; set; } //29   29: UnsignedByte: 0: iRadius
        public byte iTimeTillPedLeaves { get; set; } //30   30: UnsignedByte: 0: 2296188475  //in game minutes?
        public byte Unused6 { get; set; }//31
        public ushort iScenarioGroup { get; set; } //32   32: UnsignedShort: 0: iScenarioGroup//2180252673
        public ushort Unused7 { get; set; }//34
        public Unk_700327466 Flags { get; set; } //36   36: IntFlags2: 700327466: Flags
        public uint Unused8 { get; set; }//40
        public uint Unused9 { get; set; }//44
        public Vector4 vPositionAndDirection { get; set; } //48   48: Float_XYZW: 0: vPositionAndDirection//4685037

        public override string ToString()
        {
            return FloatUtil.GetVector4String(vPositionAndDirection); //iTimeStartOverride.ToString() + "-" + iTimeEndOverride.ToString();// + ", " + Flags.ToString();
        }
    }
    [TC(typeof(EXP))] public class MCScenarioPoint : MetaWrapper
    {
        [TC(typeof(EXP))] public MCScenarioPointContainer Container { get; set; }
        public MCScenarioPointRegion Region { get; set; }

        public CScenarioPoint _Data;
        public CScenarioPoint Data { get { return _Data; } set { _Data = value; } }

        public Vector3 Position { get { return _Data.vPositionAndDirection.XYZ(); } set { _Data.vPositionAndDirection = new Vector4(value, Direction); } }
        public float Direction { get { return _Data.vPositionAndDirection.W; } set { _Data.vPositionAndDirection = new Vector4(Position, value); } }
        public Quaternion Orientation
        {
            get { return Quaternion.RotationAxis(Vector3.UnitZ, Direction); }
            set
            {
                Vector3 dir = value.Multiply(Vector3.UnitX);
                float dira = (float)Math.Atan2(dir.Y, dir.X);
                Direction = dira;
            }
        }


        public byte TypeId { get { return _Data.iType; } set { _Data.iType = value; } }
        public ScenarioType Type { get; set; }

        public byte ModelSetId { get { return _Data.ModelSetId; } set { _Data.ModelSetId = value; } }
        public AmbientModelSet ModelSet { get; set; }

        public byte InteriorId {  get { return _Data.iInterior; } set { _Data.iInterior = value; } }
        public MetaHash InteriorName { get; set; }

        public ushort GroupId { get { return _Data.iScenarioGroup; } set { _Data.iScenarioGroup = value; } }
        public MetaHash GroupName { get; set; }

        public byte IMapId { get { return _Data.iRequiredIMapId; } set { _Data.iRequiredIMapId = value; } }
        public MetaHash IMapName { get; set; }

        public string TimeRange { get { return _Data.iTimeStartOverride.ToString().PadLeft(2, '0') + ":00 - " + _Data.iTimeEndOverride.ToString().PadLeft(2, '0') + ":00"; } }
        public byte TimeStart { get { return _Data.iTimeStartOverride; } set { _Data.iTimeStartOverride = value; } }
        public byte TimeEnd { get { return _Data.iTimeEndOverride; } set { _Data.iTimeEndOverride = value; } }
        public byte Probability { get { return _Data.iProbability; } set { _Data.iProbability = value; } }
        public byte AvailableMpSp { get { return _Data.uAvailableInMpSp; } set { _Data.uAvailableInMpSp = value; } }
        public byte Radius { get { return _Data.iRadius; } set { _Data.iRadius = value; } }
        public byte WaitTime { get { return _Data.iTimeTillPedLeaves; } set { _Data.iTimeTillPedLeaves = value; } }
        public Unk_700327466 Flags { get { return _Data.Flags; } set { _Data.Flags = value; } }

        public int PointIndex { get; set; }


        public MCScenarioPoint(MCScenarioPointRegion region) { Region = region; }
        public MCScenarioPoint(MCScenarioPointRegion region, Meta meta, CScenarioPoint d, MCScenarioPointContainer container)
        {
            Region = region;
            Container = container;
            _Data = d;
        }
        public MCScenarioPoint(MCScenarioPointRegion region, MCScenarioPoint copy)
        {
            Region = region;
            if (copy != null)
            {
                _Data = copy.Data;
                Type = copy.Type;
                ModelSet = copy.ModelSet;
                InteriorName = copy.InteriorName;
                GroupName = copy.GroupName;
                IMapName = copy.IMapName;
            }
        }

        public void CopyFrom(MCScenarioPoint copy)
        {
            _Data = copy.Data;
            Type = copy.Type;
            ModelSet = copy.ModelSet;
            InteriorName = copy.InteriorName;
            GroupName = copy.GroupName;
            IMapName = copy.IMapName;
        }


        public override void Load(Meta meta, MetaPOINTER ptr)
        {
            _Data = MetaTypes.GetData<CScenarioPoint>(meta, ptr);
        }

        public override MetaPOINTER Save(MetaBuilder mb)
        {
            mb.AddStructureInfo(MetaName.CScenarioPoint);
            return mb.AddItemPtr(MetaName.CScenarioPoint, _Data);
        }

        public override string Name
        {
            get
            {
                return (Type?.ToString() ?? "") + ": " + (ModelSet?.ToString() ?? "");
            }
        }

        public override string ToString()
        {
            return Name + ": " + TimeRange;
        }

    }

    [TC(typeof(EXP))] public struct CScenarioEntityOverride  //SCENARIO Entity Override //80 bytes, Key:1271200492
    {
        public Vector3 EntityPosition { get; set; } //0   0: Float_XYZ: 0: EntityPosition//642078041
        public float Unused00 { get; set; }//12
        public MetaHash EntityType { get; set; } //16   16: Hash: 0: EntityType//1374199246
        public uint Unused01 { get; set; }//20
        public Array_Structure ScenarioPoints { get; set; } //24   24: Array: 0: ScenarioPoints  {0: Structure: CExtensionDefSpawnPoint: 256}
        public uint Unused02 { get; set; }//40
        public uint Unused03 { get; set; }//44
        public uint Unused04 { get; set; }//48
        public uint Unused05 { get; set; }//52
        public uint Unused06 { get; set; }//56
        public uint Unused07 { get; set; }//60
        public byte Unk_538733109 { get; set; } //64   64: Boolean: 0: 538733109
        public byte Unk_1035513142 { get; set; } //65   65: Boolean: 0: 1035513142
        public ushort Unused08 { get; set; }//66
        public uint Unused09 { get; set; }//68
        public uint Unused10 { get; set; }//72
        public uint Unused11 { get; set; }//76

        public override string ToString()
        {
            return EntityType.ToString() + ", " + ScenarioPoints.Count1.ToString() + " ScenarioPoints";
        }
    }
    [TC(typeof(EXP))] public class MCScenarioEntityOverride : MetaWrapper
    {
        [TC(typeof(EXP))] public object Parent { get; set; }
        public MCScenarioPointRegion Region { get; set; }

        public CScenarioEntityOverride _Data;
        public CScenarioEntityOverride Data { get { return _Data; } set { _Data = value; } }

        public Vector3 Position { get { return _Data.EntityPosition; } set { _Data.EntityPosition = value; } }

        public MetaHash TypeName {  get { return _Data.EntityType; } set { _Data.EntityType = value; } }
        public byte Unk1 { get { return _Data.Unk_538733109; } set { _Data.Unk_538733109 = value; } }
        public byte Unk2 { get { return _Data.Unk_1035513142; } set { _Data.Unk_1035513142 = value; } }


        public MCExtensionDefSpawnPoint[] ScenarioPoints { get; set; }

        public MCScenarioEntityOverride() { }
        public MCScenarioEntityOverride(MCScenarioPointRegion region, MCScenarioEntityOverride copy)
        {
            Region = region;
            if (copy != null)
            {
                _Data = copy.Data;
            }
        }
        public MCScenarioEntityOverride(MCScenarioPointRegion region, Meta meta, CScenarioEntityOverride d)
        {
            Region = region;
            _Data = d;
            Init(meta);
        }

        public void Init(Meta meta)
        {

            var scenarioPoints = MetaTypes.ConvertDataArray<CExtensionDefSpawnPoint>(meta, MetaName.CExtensionDefSpawnPoint, _Data.ScenarioPoints);
            if (scenarioPoints != null)
            {
                ScenarioPoints = new MCExtensionDefSpawnPoint[scenarioPoints.Length];
                for (int i = 0; i < scenarioPoints.Length; i++)
                {
                    ScenarioPoints[i] = new MCExtensionDefSpawnPoint(Region, meta, scenarioPoints[i], this);
                    ScenarioPoints[i].ParentPosition = Position;
                }
            }

        }

        public override void Load(Meta meta, MetaPOINTER ptr)
        {
            _Data = MetaTypes.GetData<CScenarioEntityOverride>(meta, ptr);
            Init(meta);
        }

        public override MetaPOINTER Save(MetaBuilder mb)
        {
            mb.AddStructureInfo(MetaName.CScenarioEntityOverride);

            if (ScenarioPoints != null)
            {
                mb.AddStructureInfo(MetaName.CExtensionDefSpawnPoint);
                mb.AddEnumInfo((MetaName)3573596290);
                mb.AddEnumInfo((MetaName)700327466);
                _Data.ScenarioPoints = mb.AddWrapperArray(ScenarioPoints);
            }

            return mb.AddItemPtr(MetaName.CScenarioEntityOverride, _Data);
        }


        public void AddScenarioPoint(MCExtensionDefSpawnPoint p)
        {
            List<MCExtensionDefSpawnPoint> newpoints = new List<MCExtensionDefSpawnPoint>();
            if (ScenarioPoints != null)
            {
                newpoints.AddRange(ScenarioPoints);
            }
            newpoints.Add(p);
            p.Parent = this;
            ScenarioPoints = newpoints.ToArray();
        }
        public bool RemoveScenarioPoint(MCExtensionDefSpawnPoint p)
        {
            bool r = false;
            if (ScenarioPoints != null)
            {
                List<MCExtensionDefSpawnPoint> newpoints = new List<MCExtensionDefSpawnPoint>();
                foreach (var mp in ScenarioPoints)
                {
                    if (mp == p)
                    {
                        r = true;
                    }
                    else
                    {
                        newpoints.Add(mp);
                    }
                }
                if (r)
                {
                    ScenarioPoints = newpoints.ToArray();
                }
            }
            return r;
        }


        public CExtensionDefSpawnPoint[] GetCScenarioPoints()
        {
            if ((ScenarioPoints == null) || (ScenarioPoints.Length == 0)) return null;
            CExtensionDefSpawnPoint[] r = new CExtensionDefSpawnPoint[ScenarioPoints.Length];
            for (int i = 0; i < ScenarioPoints.Length; i++)
            {
                r[i] = ScenarioPoints[i].Data;
            }
            return r;
        }



        public override string Name
        {
            get
            {
                return "CScenarioEntityOverride " + _Data.EntityType.ToString();
            }
        }

        public override string ToString()
        {
            return _Data.ToString();
        }
    }

    [TC(typeof(EXP))] public struct Unk_4023740759  //SCENARIO PATH ARRAYS //88 bytes, Key:88255871
    {
        public Array_Structure Nodes { get; set; } //0   0: Array: 0: Nodes  {0: Structure: CScenarioChainingNode//3340683255: 256}
        public Array_Structure Edges { get; set; } //16   16: Array: 0: Edges  {0: Structure: CScenarioChainingEdge//4255409560: 256}
        public Array_Structure Chains { get; set; } //32   32: Array: 0: Chains  {0: Structure: CScenarioChain: 256}
        public uint Unused0 { get; set; }//48
        public uint Unused1 { get; set; }//52
        public uint Unused2 { get; set; }//56
        public uint Unused3 { get; set; }//60
        public uint Unused4 { get; set; }//64
        public uint Unused5 { get; set; }//68
        public uint Unused6 { get; set; }//72
        public uint Unused7 { get; set; }//76
        public uint Unused8 { get; set; }//80
        public uint Unused9 { get; set; }//84

        public override string ToString()
        {
            return Nodes.Count1.ToString() + " Nodes, " + Edges.Count1.ToString() + " Edges, " + Chains.Count1.ToString() + " Chains";
        }
    }
    [TC(typeof(EXP))] public class MUnk_4023740759 : MetaWrapper
    {
        public MCScenarioPointRegion Region { get; private set; }

        public Unk_4023740759 _Data;
        public Unk_4023740759 Data { get { return _Data; } set { _Data = value; } }

        public MCScenarioChainingNode[] Nodes { get; set; }
        public MCScenarioChainingEdge[] Edges { get; set; }
        public MCScenarioChain[] Chains { get; set; }

        public MUnk_4023740759() { }
        public MUnk_4023740759(MCScenarioPointRegion region) { Region = region; }
        public MUnk_4023740759(MCScenarioPointRegion region, Meta meta, Unk_4023740759 d)
        {
            Region = region;
            _Data = d;
            Init(meta);
        }

        public void Init(Meta meta)
        {
            var pathnodes = MetaTypes.ConvertDataArray<CScenarioChainingNode>(meta, MetaName.CScenarioChainingNode, _Data.Nodes);
            if (pathnodes != null)
            {
                Nodes = new MCScenarioChainingNode[pathnodes.Length];
                for (int i = 0; i < pathnodes.Length; i++)
                {
                    Nodes[i] = new MCScenarioChainingNode(Region, meta, pathnodes[i], this, i);
                }
            }

            var pathedges = MetaTypes.ConvertDataArray<CScenarioChainingEdge>(meta, MetaName.CScenarioChainingEdge, _Data.Edges);
            if (pathedges != null)
            {
                Edges = new MCScenarioChainingEdge[pathedges.Length];
                for (int i = 0; i < pathedges.Length; i++)
                {
                    Edges[i] = new MCScenarioChainingEdge(Region, meta, pathedges[i], i);
                }
            }

            var pathchains = MetaTypes.ConvertDataArray<CScenarioChain>(meta, MetaName.CScenarioChain, _Data.Chains);
            if (pathchains != null)
            {
                Chains = new MCScenarioChain[pathchains.Length];
                for (int i = 0; i < pathchains.Length; i++)
                {
                    Chains[i] = new MCScenarioChain(Region, meta, pathchains[i]);
                }
            }
        }

        public override void Load(Meta meta, MetaPOINTER ptr)
        {
            _Data = MetaTypes.GetData<Unk_4023740759>(meta, ptr);
            Init(meta);
        }



        public override MetaPOINTER Save(MetaBuilder mb)
        {
            mb.AddStructureInfo((MetaName)4023740759);
            return mb.AddItemPtr((MetaName)4023740759, _Data);
        }







        public CScenarioChainingNode[] GetCNodes()
        {
            if ((Nodes == null) || (Nodes.Length == 0)) return null;
            CScenarioChainingNode[] r = new CScenarioChainingNode[Nodes.Length];
            for (int i = 0; i < Nodes.Length; i++)
            {
                r[i] = Nodes[i].Data;
            }
            return r;
        }
        public CScenarioChainingEdge[] GetCEdges()
        {
            if ((Edges == null) || (Edges.Length == 0)) return null;
            CScenarioChainingEdge[] r = new CScenarioChainingEdge[Edges.Length];
            for (int i = 0; i < Edges.Length; i++)
            {
                r[i] = Edges[i].Data;
            }
            return r;
        }
        public CScenarioChain[] GetCChains()
        {
            if ((Chains == null) || (Chains.Length == 0)) return null;
            CScenarioChain[] r = new CScenarioChain[Chains.Length];
            for (int i = 0; i < Chains.Length; i++)
            {
                r[i] = Chains[i].Data;
            }
            return r;
        }



        public void AddNode(MCScenarioChainingNode node)
        {
            List<MCScenarioChainingNode> newnodes = new List<MCScenarioChainingNode>();
            if (Nodes != null)
            {
                newnodes.AddRange(Nodes);
            }
            node.Parent = this;
            node.Region = Region;
            node.NodeIndex = newnodes.Count;
            newnodes.Add(node);
            Nodes = newnodes.ToArray();
        }
        public void AddEdge(MCScenarioChainingEdge edge)
        {
            List<MCScenarioChainingEdge> newedges = new List<MCScenarioChainingEdge>();
            if (Edges != null)
            {
                newedges.AddRange(Edges);
            }
            edge.Region = Region;
            edge.EdgeIndex = newedges.Count;
            newedges.Add(edge);
            Edges = newedges.ToArray();
        }
        public void AddChain(MCScenarioChain chain)
        {
            List<MCScenarioChain> newchains = new List<MCScenarioChain>();
            if (Chains != null)
            {
                newchains.AddRange(Chains);
            }
            chain.Region = Region;
            chain.ChainIndex = newchains.Count;
            newchains.Add(chain);
            Chains = newchains.ToArray();
        }

        public bool RemoveNode(MCScenarioChainingNode n)
        {
            bool r = false;
            if (Edges != null)
            {
                //first remove any edges referencing this node...
                List<MCScenarioChainingEdge> remedges = new List<MCScenarioChainingEdge>();
                if (Chains != null)
                {
                    foreach (var chain in Chains)
                    {
                        if (chain.Edges == null) continue;
                        remedges.Clear();
                        foreach (var edge in chain.Edges)
                        {
                            if ((edge.NodeFrom == n) || (edge.NodeTo == n))
                            {
                                remedges.Add(edge);
                            }
                        }
                        foreach (var edge in remedges)
                        {
                            chain.RemoveEdge(edge);
                        }
                    }
                }
                remedges.Clear();
                foreach (var edge in Edges)
                {
                    if ((edge.NodeFrom == n) || (edge.NodeTo == n))
                    {
                        remedges.Add(edge);
                    }
                }
                foreach (var edge in remedges)
                {
                    RemoveEdge(edge);
                }
            }

            if (Nodes != null)
            {
                List<MCScenarioChainingNode> newnodes = new List<MCScenarioChainingNode>();
                foreach (var nn in Nodes)
                {
                    if (nn == n)
                    {
                        r = true;
                    }
                    else
                    {
                        nn.NodeIndex = newnodes.Count;
                        newnodes.Add(nn);
                    }
                }
                if (r)
                {
                    Nodes = newnodes.ToArray();

                    foreach (var e in Edges)
                    {
                        e.NodeIndexFrom = (ushort)e.NodeFrom.NodeIndex;
                        e.NodeIndexTo = (ushort)e.NodeTo.NodeIndex;
                    }

                }
            }
            return r;
        }
        public bool RemoveEdge(MCScenarioChainingEdge e)
        {
            bool r = false;
            if (Edges != null)
            {
                List<MCScenarioChainingEdge> newedges = new List<MCScenarioChainingEdge>();
                foreach (var ne in Edges)
                {
                    if (ne == e)
                    {
                        r = true;
                    }
                    else
                    {
                        ne.EdgeIndex = newedges.Count;
                        newedges.Add(ne);
                    }
                }
                if (r)
                {
                    Edges = newedges.ToArray();

                    foreach (var c in Chains)
                    {
                        if ((c?.Edges != null) && (c?.EdgeIds != null))
                        {
                            for (int i = 0; i < c.Edges.Length; i++)
                            {
                                c.EdgeIds[i] = (ushort)c.Edges[i].EdgeIndex;
                            }
                        }
                    }
                }
            }
            return r;
        }
        public bool RemoveChain(MCScenarioChain c)
        {
            bool r = false;
            if (Chains != null)
            {
                List<MCScenarioChain> newchains = new List<MCScenarioChain>();
                foreach (var nc in Chains)
                {
                    if (nc == c)
                    {
                        r = true;
                    }
                    else
                    {
                        nc.ChainIndex = newchains.Count;
                        newchains.Add(nc);
                    }
                }
                if (r)
                {
                    Chains = newchains.ToArray();
                }
            }
            return r;
        }



        public override string Name
        {
            get
            {
                return "Unk_4023740759 (Scenario paths) " + _Data.ToString();
            }
        }
        public override string ToString()
        {
            return _Data.ToString();
        }
    }

    [TC(typeof(EXP))] public struct CScenarioChainingNode //SCENARIO PATH NODE //32 bytes, Key:1811784424
    {
        public Vector3 Position { get; set; } //0   0: Float_XYZ: 0: Position
        public float Unused0 { get; set; }//12
        public MetaHash Unk_2602393771 { get; set; } //16   16: Hash: 0: 2602393771
        public MetaHash ScenarioType { get; set; } //20   20: Hash: 0: ScenarioType
        public byte Unk_407126079_NotFirst { get; set; } //24   24: Boolean: 0: 407126079 //can move backwards? (not first node)
        public byte Unk_1308720135_NotLast { get; set; } //25   25: Boolean: 0: 1308720135 //can move forwards? (not last node)
        public ushort Unused1 { get; set; }//26
        public uint Unused2 { get; set; }//28

        public override string ToString()
        {
            return //Unk_407126079.ToString() + ", " + Unk_1308720135.ToString() + ", " + 
                ScenarioType.ToString() + ", " + Unk_2602393771.ToString();
        }
    }
    [TC(typeof(EXP))] public class MCScenarioChainingNode : MetaWrapper
    {
        [TC(typeof(EXP))] public MUnk_4023740759 Parent { get; set; }
        public MCScenarioPointRegion Region { get; set; }
        public ScenarioNode ScenarioNode { get; set; }

        public CScenarioChainingNode _Data;
        public CScenarioChainingNode Data { get { return _Data; } set { _Data = value; } }

        public Vector3 Position { get { return _Data.Position; } set { _Data.Position = value; } }
        public MetaHash Unk1 { get { return _Data.Unk_2602393771; } set { _Data.Unk_2602393771 = value; } }
        public MetaHash TypeHash { get { return _Data.ScenarioType; } set { _Data.ScenarioType = value; } }
        public ScenarioType Type { get; set; }
        public bool NotFirst { get { return _Data.Unk_407126079_NotFirst == 1; } set { _Data.Unk_407126079_NotFirst = (byte)(value ? 1 : 0); } }
        public bool NotLast { get { return _Data.Unk_1308720135_NotLast == 1; } set { _Data.Unk_1308720135_NotLast = (byte)(value ? 1 : 0); } }

        public int NodeIndex { get; set; }
        public MCScenarioChain Chain { get; set; }


        public MCScenarioChainingNode() { }
        public MCScenarioChainingNode(MCScenarioPointRegion region, Meta meta, CScenarioChainingNode d, MUnk_4023740759 parent, int index)
        {
            Region = region;
            Parent = parent;
            _Data = d;
            NodeIndex = index;
        }
        public MCScenarioChainingNode(MCScenarioPointRegion region, MCScenarioChainingNode copy)
        {
            Region = region;
            _Data = copy._Data;
            Type = copy.Type;
        }

        public void CopyFrom(MCScenarioChainingNode copy)
        {
            _Data = copy._Data;
            Type = copy.Type;
        }

        public override void Load(Meta meta, MetaPOINTER ptr)
        {
            _Data = MetaTypes.GetData<CScenarioChainingNode>(meta, ptr);
        }

        public override MetaPOINTER Save(MetaBuilder mb)
        {
            mb.AddStructureInfo(MetaName.CScenarioChainingNode);
            return mb.AddItemPtr(MetaName.CScenarioChainingNode, _Data);
        }

        public override string Name
        {
            get
            {
                return "CScenarioChainingNode";// + _Data.Unk_2602393771.ToString();
            }
        }


        public override string ToString()
        {
            return _Data.ToString();
        }

    }

    [TC(typeof(EXP))] public struct CScenarioChainingEdge  //SCENARIO PATH EDGE //8 bytes, Key:2004985940
    {
        public ushort NodeIndexFrom { get; set; } //0   0: UnsignedShort: 0: NodeIndexFrom//3236798246
        public ushort NodeIndexTo { get; set; } //2   2: UnsignedShort: 0: NodeIndexTo//2851806039
        public Unk_3609807418 Action { get; set; } //4   4: ByteEnum: 3609807418: Action
        public Unk_3971773454 NavMode { get; set; } //5   5: ByteEnum: 3971773454: NavMode//859022269
        public Unk_941086046 NavSpeed { get; set; } //6   6: ByteEnum: 941086046: NavSpeed//1419316113
        public byte Unused0 { get; set; }//7

        public override string ToString()
        {
            return NodeIndexFrom.ToString() + ", " + NodeIndexTo.ToString() + ", " + Action.ToString() + ", " + NavMode.ToString() + ", " + NavSpeed.ToString();
        }
    }
    [TC(typeof(EXP))] public class MCScenarioChainingEdge : MetaWrapper
    {
        public MCScenarioPointRegion Region { get; set; }

        public CScenarioChainingEdge _Data;
        public CScenarioChainingEdge Data { get { return _Data; } set { _Data = value; } }

        public MCScenarioChainingNode NodeFrom { get; set; }
        public MCScenarioChainingNode NodeTo { get; set; }
        public ushort NodeIndexFrom { get { return _Data.NodeIndexFrom; } set { _Data.NodeIndexFrom = value; } }
        public ushort NodeIndexTo { get { return _Data.NodeIndexTo; } set { _Data.NodeIndexTo = value; } }
        public Unk_3609807418 Action { get { return _Data.Action; } set { _Data.Action = value; } }
        public Unk_3971773454 NavMode { get { return _Data.NavMode; } set { _Data.NavMode = value; } }
        public Unk_941086046 NavSpeed { get { return _Data.NavSpeed; } set { _Data.NavSpeed = value; } }


        public int EdgeIndex { get; set; }

        public MCScenarioChainingEdge() { }
        public MCScenarioChainingEdge(MCScenarioPointRegion region, Meta meta, CScenarioChainingEdge d, int index)
        {
            Region = region;
            _Data = d;
            EdgeIndex = index;
        }
        public MCScenarioChainingEdge(MCScenarioPointRegion region, MCScenarioChainingEdge copy)
        {
            Region = region;
            _Data = copy._Data;
            NodeFrom = copy.NodeFrom;
            NodeTo = copy.NodeTo;//these should be updated later...
        }

        public override void Load(Meta meta, MetaPOINTER ptr)
        {
            _Data = MetaTypes.GetData<CScenarioChainingEdge>(meta, ptr);
        }

        public override MetaPOINTER Save(MetaBuilder mb)
        {
            mb.AddStructureInfo(MetaName.CScenarioChainingEdge);
            return mb.AddItemPtr(MetaName.CScenarioChainingEdge, _Data);
        }

        public override string Name
        {
            get
            {
                return "CScenarioChainingEdge";
            }
        }

        public override string ToString()
        {
            return Action.ToString() + ", " + NavMode.ToString() + ", " + NavSpeed.ToString();
        }

    }

    [TC(typeof(EXP))] public struct CScenarioChain  //SCENARIO PATH CHAIN //40 bytes, Key:2751910366
    {
        public byte Unk_1156691834 { get; set; } //0   0: UnsignedByte: 0: 1156691834
        public byte Unused0 { get; set; }//1
        public ushort Unused1 { get; set; }//2
        public uint Unused2 { get; set; }//4
        public Array_ushort EdgeIds { get; set; } //8   8: Array: 0: EdgeIds  {0: UnsignedShort: 0: 256}
        public uint Unused3 { get; set; }//24
        public uint Unused4 { get; set; }//28
        public uint Unused5 { get; set; }//32
        public uint Unused6 { get; set; }//36

        public override string ToString()
        {
            return Unk_1156691834.ToString() + ": " + EdgeIds.Count1.ToString() + " EdgeIds";
        }
    }
    [TC(typeof(EXP))] public class MCScenarioChain : MetaWrapper
    {
        public MCScenarioPointRegion Region { get; set; }

        public CScenarioChain _Data;
        public CScenarioChain Data { get { return _Data; } set { _Data = value; } }

        public byte Unk1 { get { return _Data.Unk_1156691834; } set { _Data.Unk_1156691834 = value; } }

        public ushort[] EdgeIds { get; set; }
        public MCScenarioChainingEdge[] Edges { get; set; }

        public int ChainIndex { get; set; }

        public MCScenarioChain() { }
        public MCScenarioChain(MCScenarioPointRegion region, Meta meta, CScenarioChain d)
        {
            Region = region;
            _Data = d;
            EdgeIds = MetaTypes.GetUshortArray(meta, _Data.EdgeIds);
        }

        public override void Load(Meta meta, MetaPOINTER ptr)
        {
            _Data = MetaTypes.GetData<CScenarioChain>(meta, ptr);
            EdgeIds = MetaTypes.GetUshortArray(meta, _Data.EdgeIds);
        }


        public void AddEdge(MCScenarioChainingEdge edge)
        {
            List<MCScenarioChainingEdge> newedges = new List<MCScenarioChainingEdge>();
            List<ushort> newedgeids = new List<ushort>();
            if (Edges != null)
            {
                newedges.AddRange(Edges);
            }
            if (EdgeIds != null)
            {
                newedgeids.AddRange(EdgeIds);
            }
            edge.Region = Region;
            newedges.Add(edge);
            newedgeids.Add((ushort)edge.EdgeIndex);
            Edges = newedges.ToArray();
            EdgeIds = newedgeids.ToArray();
        }
        public bool RemoveEdge(MCScenarioChainingEdge e)
        {
            bool r = false;
            if (Edges != null)
            {
                List<MCScenarioChainingEdge> newedges = new List<MCScenarioChainingEdge>();
                List<ushort> newedgeids = new List<ushort>();
                foreach (var ne in Edges)
                {
                    if (ne == e)
                    {
                        r = true;
                    }
                    else
                    {
                        newedges.Add(ne);
                        newedgeids.Add((ushort)ne.EdgeIndex);
                    }
                }
                if (r)
                {
                    Edges = newedges.ToArray();
                    EdgeIds = newedgeids.ToArray();
                }
            }
            return r;
        }


        public override MetaPOINTER Save(MetaBuilder mb)
        {
            //TODO!
            //if (EdgeIds != null)
            //{
            //    mb.AddStructureInfo(MetaName.ushort);
            //    _Data.EdgeIds = mb.AddItemArrayPtr(MetaName.ushort, EdgeIds);
            //}

            mb.AddStructureInfo(MetaName.CScenarioChain);
            return mb.AddItemPtr(MetaName.CScenarioChain, _Data);
        }

        public override string Name
        {
            get
            {
                return "CScenarioChain";
            }
        }

        public override string ToString()
        {
            return _Data.ToString();
        }

    }

    [TC(typeof(EXP))] public struct CScenarioPointCluster  //SCENARIO spawn cluster - all things spawn together //80 bytes, Key:3622480419
    {
        public CScenarioPointContainer Points { get; set; } //0   0: Structure: CScenarioPointContainer//2380938603: Points//702683191
        public rage__spdSphere ClusterSphere { get; set; } //48   48: Structure: 1062159465: ClusterSphere//352461053
        public float Unk_1095875445 { get; set; } //64   64: Float: 0: 1095875445 //spawn chance? eg 5, 30
        public byte Unk_3129415068 { get; set; } //68   68: Boolean: 0: 3129415068
        public uint Unused0 { get; set; }//72
        public uint Unused1 { get; set; }//76

        public override string ToString()
        {
            return Points.ToString();// + ", Sphere: " + ClusterSphere.ToString();
        }
    }
    [TC(typeof(EXP))] public class MCScenarioPointCluster : MetaWrapper
    {
        public MCScenarioPointRegion Region { get; set; }

        public CScenarioPointCluster _Data;
        public CScenarioPointCluster Data { get { return _Data; } set { _Data = value; } }

        public MCScenarioPointContainer Points { get; set; }

        public Vector3 Position //is separate from Points...
        {
            get { return _Data.ClusterSphere.centerAndRadius.XYZ(); }
            set
            {
                var v4 = new Vector4(value, _Data.ClusterSphere.centerAndRadius.W);
                _Data.ClusterSphere = new rage__spdSphere() { centerAndRadius = v4 };
            }
        }
        public float Radius
        {
            get { return _Data.ClusterSphere.centerAndRadius.W; }
            set
            {
                var v4 = new Vector4(_Data.ClusterSphere.centerAndRadius.XYZ(), value);
                _Data.ClusterSphere = new rage__spdSphere() { centerAndRadius = v4 };
            }
        }
        public float Unk1 { get { return _Data.Unk_1095875445; } set { _Data.Unk_1095875445 = value; } }
        public bool Unk2 { get { return _Data.Unk_3129415068==1; } set { _Data.Unk_3129415068 = (byte)(value?1:0); } }

        public MCScenarioPointCluster() { }
        public MCScenarioPointCluster(MCScenarioPointRegion region) { Region = region; }
        public MCScenarioPointCluster(MCScenarioPointRegion region, MCScenarioPointCluster copy)
        {
            Region = region;
            if (copy != null)
            {
                _Data = copy.Data;
            }
            Points = new MCScenarioPointContainer(region);
            Points.Parent = this;
        }
        public MCScenarioPointCluster(MCScenarioPointRegion region, Meta meta, CScenarioPointCluster d)
        {
            Region = region;
            _Data = d;
            Points = new MCScenarioPointContainer(region, meta, d.Points);
            Points.Parent = this;
        }

        public override void Load(Meta meta, MetaPOINTER ptr)
        {
            _Data = MetaTypes.GetData<CScenarioPointCluster>(meta, ptr);
            Points = new MCScenarioPointContainer(Region, meta, _Data.Points);
            Points.Parent = this;
        }

        public override MetaPOINTER Save(MetaBuilder mb)
        {
            mb.AddStructureInfo(MetaName.rage__spdSphere);
            mb.AddStructureInfo(MetaName.CScenarioPointCluster);


            if (Points != null)
            {
                var scp = new CScenarioPointContainer();

                ////meta version
                //if ((Points.LoadSavePoints != null) && (Points.LoadSavePoints.Length > 0))
                //{
                //    scp.LoadSavePoints = mb.AddWrapperArray(Points.LoadSavePoints);
                //}
                //if ((Points.MyPoints != null) && (Points.MyPoints.Length > 0))
                //{
                //    scp.MyPoints = mb.AddWrapperArray(Points.MyPoints);
                //}

                //optimised version...
                var loadSavePoints = Points.GetCLoadSavePoints();
                if (loadSavePoints != null)//this never seems to be used...
                {
                    mb.AddStructureInfo(MetaName.CExtensionDefSpawnPoint);
                    mb.AddEnumInfo((MetaName)3573596290);
                    mb.AddEnumInfo((MetaName)700327466);
                    scp.LoadSavePoints = mb.AddItemArrayPtr(MetaName.CExtensionDefSpawnPoint, loadSavePoints);
                }
                var myPoints = Points.GetCMyPoints();
                if (myPoints != null)
                {
                    mb.AddStructureInfo(MetaName.CScenarioPoint);
                    mb.AddEnumInfo((MetaName)700327466);
                    scp.MyPoints = mb.AddItemArrayPtr(MetaName.CScenarioPoint, myPoints);
                }

                _Data.Points = scp;
            }
            else
            {
                _Data.Points = new CScenarioPointContainer();
            }

            return mb.AddItemPtr(MetaName.CScenarioPointCluster, _Data);
        }

        public override string Name
        {
            get { return "CScenarioPointCluster"; }
        }


        public override string ToString()
        {
            return _Data.ToString();
        }
    }

    [TC(typeof(EXP))] public struct CScenarioPointLookUps  //SCENARIO hash arrays //96 bytes, Key:2669361587
    {
        public Array_uint TypeNames { get; set; } //0   0: Array: 0: TypeNames//3057471271  {0: Hash: 0: 256}
        public Array_uint PedModelSetNames { get; set; } //16   16: Array: 0: PedModelSetNames//3020866217  {0: Hash: 0: 256}
        public Array_uint VehicleModelSetNames { get; set; } //32   32: Array: 0: VehicleModelSetNames//3827910541  {0: Hash: 0: 256}
        public Array_uint GroupNames { get; set; } //48   48: Array: 0: GroupNames//2506712617  {0: Hash: 0: 256}
        public Array_uint InteriorNames { get; set; } //64   64: Array: 0: InteriorNames  {0: Hash: 0: 256}
        public Array_uint RequiredIMapNames { get; set; } //[ymap names] //80   80: Array: 0: RequiredIMapNames//1767860162  {0: Hash: 0: 256}

        public override string ToString()
        {
            return "CScenarioPointLookUps";
        }
    }
    [TC(typeof(EXP))] public class MCScenarioPointLookUps : MetaWrapper
    {
        public MCScenarioPointRegion Region { get; set; }

        public CScenarioPointLookUps _Data;
        public CScenarioPointLookUps Data { get { return _Data; } set { _Data = value; } }

        public MetaHash[] TypeNames { get; set; } //scenario type hashes used by points
        public MetaHash[] PedModelSetNames { get; set; } //ped names
        public MetaHash[] VehicleModelSetNames { get; set; } //vehicle names
        public MetaHash[] GroupNames { get; set; }  //scenario group names?
        public MetaHash[] InteriorNames { get; set; }
        public MetaHash[] RequiredIMapNames { get; set; } //ymap names


        public MCScenarioPointLookUps() { }
        public MCScenarioPointLookUps(MCScenarioPointRegion region)
        {
            Region = region;
        }
        public MCScenarioPointLookUps(MCScenarioPointRegion region, Meta meta, CScenarioPointLookUps d)
        {
            Region = region;
            _Data = d;
            Init(meta);
        }

        public void Init(Meta meta)
        {
            TypeNames = MetaTypes.GetHashArray(meta, _Data.TypeNames);
            PedModelSetNames = MetaTypes.GetHashArray(meta, _Data.PedModelSetNames);
            VehicleModelSetNames = MetaTypes.GetHashArray(meta, _Data.VehicleModelSetNames);
            GroupNames = MetaTypes.GetHashArray(meta, _Data.GroupNames);
            InteriorNames = MetaTypes.GetHashArray(meta, _Data.InteriorNames);
            RequiredIMapNames = MetaTypes.GetHashArray(meta, _Data.RequiredIMapNames);
        }

        public override void Load(Meta meta, MetaPOINTER ptr)
        {
            _Data = MetaTypes.GetData<CScenarioPointLookUps>(meta, ptr);
            Init(meta);
        }

        public override MetaPOINTER Save(MetaBuilder mb)
        {
            mb.AddStructureInfo(MetaName.CScenarioPointLookUps);
            return mb.AddItemPtr(MetaName.CScenarioPointLookUps, _Data);
        }

        public override string Name
        {
            get
            {
                return "CScenarioPointLookUps";
            }
        }

        public override string ToString()
        {
            return _Data.ToString();
        }

    }





















    [TC(typeof(EXP))] public struct CCompositeEntityType //304 bytes, Key:659539004 dex: composite entity type - ytyp // Tom: des_ destruction
    {
        public ArrayOfChars64 Name { get; set; } //0   0: ArrayOfChars: 64: Name
        public float lodDist { get; set; } //64   64: Float: 0: lodDist
        public uint flags { get; set; } //68   68: UnsignedInt: 0: flags
        public uint specialAttribute { get; set; } //72   72: UnsignedInt: 0: specialAttribute
        public uint Unused0 { get; set; }//76
        public Vector3 bbMin { get; set; } //80   80: Float_XYZ: 0: bbMin
        public float Unused1 { get; set; }//92
        public Vector3 bbMax { get; set; } //96   96: Float_XYZ: 0: bbMax
        public float Unused2 { get; set; }//108
        public Vector3 bsCentre { get; set; } //112   112: Float_XYZ: 0: bsCentre
        public float Unused3 { get; set; }//124
        public float bsRadius { get; set; } //128   128: Float: 0: bsRadius
        public uint Unused4 { get; set; }//132
        public ArrayOfChars64 StartModel { get; set; } //136   136: ArrayOfChars: 64: StartModel
        public ArrayOfChars64 EndModel { get; set; } //200   200: ArrayOfChars: 64: EndModel
        public MetaHash StartImapFile { get; set; } //264   264: Hash: 0: StartImapFile//2462971690
        public MetaHash EndImapFile { get; set; } //268   268: Hash: 0: EndImapFile//2059586669
        public MetaHash PtFxAssetName { get; set; } //272   272: Hash: 0: PtFxAssetName//2497993358
        public uint Unused5 { get; set; }//276
        public Array_Structure Animations { get; set; } //280   280: Array: 0: Animations  {0: Structure: 1980345114: 256}
        public uint Unused6 { get; set; }//296
        public uint Unused7 { get; set; }//300

        public override string ToString()
        {
            return Name.ToString() + ", " + StartModel.ToString() + ", " + EndModel.ToString() + ", " + 
                    StartImapFile.ToString() + ", " + EndImapFile.ToString() + ", " + PtFxAssetName.ToString();
        }
    }

    [TC(typeof(EXP))] public struct Unk_1980345114 //216 bytes, Key:4110496011 //destruction animations?
    {
        public ArrayOfChars64 AnimDict { get; set; } //0   0: ArrayOfChars: 64: AnimDict
        public ArrayOfChars64 AnimName { get; set; } //64   64: ArrayOfChars: 64: AnimName
        public ArrayOfChars64 AnimatedModel { get; set; } //128   128: ArrayOfChars: 64: AnimatedModel
        public float punchInPhase { get; set; } //192   192: Float: 0: punchInPhase//3142377407
        public float punchOutPhase { get; set; } //196   196: Float: 0: punchOutPhase//2164219370
        public Array_Structure effectsData { get; set; } //200   200: Array: 0: effectsData  {0: Structure: 3430328684: 256}
    }

    [TC(typeof(EXP))] public struct Unk_3430328684 //160 bytes, Key:1724963966 //destruction animation effects data
    {
        public uint fxType { get; set; } //0   0: UnsignedInt: 0: fxType
        public uint Unused0 { get; set; }//4
        public uint Unused1 { get; set; }//8
        public uint Unused2 { get; set; }//12
        public Vector3 fxOffsetPos { get; set; } //16   16: Float_XYZ: 0: fxOffsetPos
        public float Unused3 { get; set; }//28
        public Vector4 fxOffsetRot { get; set; } //32   32: Float_XYZW: 0: fxOffsetRot
        public uint boneTag { get; set; } //48   48: UnsignedInt: 0: boneTag
        public float startPhase { get; set; } //52   52: Float: 0: startPhase
        public float endPhase { get; set; } //56   56: Float: 0: endPhase
        public byte ptFxIsTriggered { get; set; } //60   60: Boolean: 0: ptFxIsTriggered
        public ArrayOfChars64 ptFxTag { get; set; } //61   61: ArrayOfChars: 64: ptFxTag
        public byte Unused4 { get; set; }//125
        public ushort Unused5 { get; set; }//126
        public float ptFxScale { get; set; } //128   128: Float: 0: ptFxScale
        public float ptFxProbability { get; set; } //132   132: Float: 0: ptFxProbability
        public byte ptFxHasTint { get; set; } //136   136: Boolean: 0: ptFxHasTint
        public byte ptFxTintR { get; set; } //137   137: UnsignedByte: 0: ptFxTintR
        public byte ptFxTintG { get; set; } //138   138: UnsignedByte: 0: ptFxTintG
        public byte ptFxTintB { get; set; } //139   139: UnsignedByte: 0: ptFxTintB
        public uint Unused6 { get; set; }//140
        public Vector3 ptFxSize { get; set; } //144   144: Float_XYZ: 0: ptFxSize
        public uint Unused7 { get; set; }//156
    }












    public struct CStreamingRequestRecord //40 bytes, Key:3825587854  //SRL YMT ROOT - in /streaming/ folder
    {
        public Array_Structure Frames { get; set; } //0   0: Array: 0: Frames//419044527  {0: Structure: CStreamingRequestFrame//999226379: 256}
        public Array_Structure CommonSets { get; set; } //16   16: Array: 0: CommonSets//4248405899  {0: Structure: 1358189812: 256}
        public byte NewStyle { get; set; } //32   32: Boolean: 0: 2333392588
        public byte Unused0 { get; set; }//33
        public ushort Unused1 { get; set; }//34
        public uint Unused2 { get; set; }//36
    }

    public struct CStreamingRequestFrame //112 bytes, Key:1112444512  //SRL frame...
    {
        public Array_uint AddList { get; set; } //0   0: Array: 0: AddList//327274266  {0: Hash: 0: 256}
        public Array_uint RemoveList { get; set; } //16   16: Array: 0: RemoveList//3372321331  {0: Hash: 0: 256}
        public Array_uint Unk_896120921 { get; set; } //32   32: Array: 0: 896120921  {0: Hash: 0: 256}
        public Vector3 CamPos { get; set; } //48   48: Float_XYZ: 0: CamPos//357008256
        public float Unused0 { get; set; }//60
        public Vector3 CamDir { get; set; } //64   64: Float_XYZ: 0: CamDir//210316193
        public float Unused1 { get; set; }//76
        public Array_byte Unk_1762439591 { get; set; } //80   80: Array: 0: 1762439591  {0: UnsignedByte: 0: 256}
        public uint Flags { get; set; } //96   96: UnsignedInt: 0: Flags
        public uint Unused2 { get; set; }//100
        public uint Unused3 { get; set; }//104
        public uint Unused4 { get; set; }//108
    }

    public struct CStreamingRequestFrame_v2 //96 bytes, Key:3672937465  //SRL frame...
    {
        public Array_uint AddList { get; set; } //0   0: Array: 0: AddList//327274266  {0: Hash: 0: 256}
        public Array_uint RemoveList { get; set; } //16   16: Array: 0: RemoveList//3372321331  {0: Hash: 0: 256}
        public Vector3 CamPos { get; set; } //32   32: Float_XYZ: 0: CamPos//357008256
        public float Unused0 { get; set; }//44
        public Vector3 CamDir { get; set; } //48   48: Float_XYZ: 0: CamDir//210316193
        public float Unused1 { get; set; }//60
        public Array_byte Unk_1762439591 { get; set; } //64   64: Array: 0: 1762439591  {0: UnsignedByte: 0: 256}
        public uint Flags { get; set; } //80   80: UnsignedInt: 0: Flags
        public uint Unused2 { get; set; }//84
        public uint Unused3 { get; set; }//88
        public uint Unused4 { get; set; }//92
    }

    public struct Unk_1358189812 //16 bytes, Key:3710200606   //SRL hashes list?
    {
        public Array_uint Requests { get; set; } //0   0: Array: 0: Requests//2743119154  {0: Hash: 0: 256}
    }














    public struct CCreatureMetaData //56 bytes, Key:2181653572
    {
        public uint Unused0 { get; set; }//0
        public uint Unused1 { get; set; }//4
        public Array_Structure shaderVariableComponents { get; set; } //8   8: Array: 0: shaderVariableComponents  {0: Structure: CShaderVariableComponent: 256}
        public Array_Structure pedPropExpressions { get; set; } //24   24: Array: 0: pedPropExpressions  {0: Structure: CPedPropExpressionData: 256}
        public Array_Structure pedCompExpressions { get; set; } //40   40: Array: 0: pedCompExpressions  {0: Structure: CPedCompExpressionData: 256}
    }

    public struct CShaderVariableComponent //72 bytes, Key:3085831725
    {
        public uint Unused0 { get; set; }//0
        public uint Unused1 { get; set; }//4
        public uint pedcompID { get; set; } //8   8: UnsignedInt: 0: pedcompID
        public uint maskID { get; set; } //12   12: UnsignedInt: 0: maskID
        public uint shaderVariableHashString { get; set; } //16   16: Hash: 0: shaderVariableHashString
        public uint Unused2 { get; set; }//20
        public Array_byte tracks { get; set; } //24   24: Array: 0: tracks  {0: UnsignedByte: 0: 256}
        public Array_ushort ids { get; set; } //40   40: Array: 0: ids  {0: UnsignedShort: 0: 256}
        public Array_byte components { get; set; } //56   56: Array: 0: components  {0: UnsignedByte: 0: 256}
    }

    public struct CPedPropExpressionData //88 bytes, Key:1355135810
    {
        public uint Unused0 { get; set; }//0
        public uint Unused1 { get; set; }//4
        public uint pedPropID { get; set; } //8   8: UnsignedInt: 0: pedPropID
        public int pedPropVarIndex { get; set; } //12   12: SignedInt: 0: pedPropVarIndex
        public uint pedPropExpressionIndex { get; set; } //16   16: UnsignedInt: 0: pedPropExpressionIndex
        public uint Unused2 { get; set; }//20
        public Array_byte tracks { get; set; } //24   24: Array: 0: tracks  {0: UnsignedByte: 0: 256}
        public Array_ushort ids { get; set; } //40   40: Array: 0: ids  {0: UnsignedShort: 0: 256}
        public Array_byte types { get; set; } //56   56: Array: 0: types  {0: UnsignedByte: 0: 256}
        public Array_byte components { get; set; } //72   72: Array: 0: components  {0: UnsignedByte: 0: 256}
    }

    public struct CPedCompExpressionData //88 bytes, Key:3458164745
    {
        public uint Unused0 { get; set; }//0
        public uint Unused1 { get; set; }//4
        public uint pedCompID { get; set; } //8   8: UnsignedInt: 0: pedCompID
        public int pedCompVarIndex { get; set; } //12   12: SignedInt: 0: pedCompVarIndex
        public uint pedCompExpressionIndex { get; set; } //16   16: UnsignedInt: 0: pedCompExpressionIndex
        public uint Unused2 { get; set; }//20
        public Array_byte tracks { get; set; } //24   24: Array: 0: tracks  {0: UnsignedByte: 0: 256}
        public Array_ushort ids { get; set; } //40   40: Array: 0: ids  {0: UnsignedShort: 0: 256}
        public Array_byte types { get; set; } //56   56: Array: 0: types  {0: UnsignedByte: 0: 256}
        public Array_byte components { get; set; } //72   72: Array: 0: components  {0: UnsignedByte: 0: 256}
    }




    public struct Unk_376833625 //112 bytes, Key:4030871161  //COMPONENT PEDS YMT ROOT  - in componentpeds .rpf's
    {
        public byte Unk_1235281004 { get; set; } //0   0: Boolean: 0: 1235281004
        public byte Unk_4086467184 { get; set; } //1   1: Boolean: 0: 4086467184
        public byte Unk_911147899 { get; set; } //2   2: Boolean: 0: 911147899
        public byte Unk_315291935 { get; set; } //3   3: Boolean: 0: 315291935
        public ArrayOfBytes12 Unk_2996560424 { get; set; } //4   4: ArrayOfBytes: 12: 2996560424
        public Array_Structure Unk_3796409423 { get; set; } //16   16: Array: 0: 3796409423  {0: Structure: 3538495220: 256}
        public Array_Structure Unk_2131007641 { get; set; } //32   32: Array: 0: 2131007641  {0: Structure: 253191135: 256}
        public Array_Structure compInfos { get; set; } //48   48: Array: 0: compInfos//592652859  {0: Structure: CComponentInfo//1866571721: 256}
        public Unk_2858946626 propInfo { get; set; } //64   64: Structure: 2858946626: propInfo//2240851416
        public MetaHash dlcName { get; set; } //104   104: Hash: 0: dlcName
        public uint Unused0 { get; set; }//108
    }

    public struct Unk_3538495220 //24 bytes, Key:2024084511  //COMPONENT PEDS unknown
    {
        public byte Unk_3371516811 { get; set; } //0   0: UnsignedByte: 0: 3371516811
        public byte Unused0 { get; set; }//1
        public ushort Unused1 { get; set; }//2
        public uint Unused2 { get; set; }//4
        public Array_Structure Unk_1756136273 { get; set; } //8   8: Array: 0: 1756136273  {0: Structure: 1535046754: 256}
    }

    public struct Unk_1535046754 //48 bytes, Key:124073662  //COMPONENT PEDS unknown /cloth?
    {
        public byte propMask { get; set; } //0   0: UnsignedByte: 0: propMask//2932859459
        public byte Unk_2806194106 { get; set; } //1   1: UnsignedByte: 0: 2806194106
        public ushort Unused0 { get; set; }//2
        public uint Unused1 { get; set; }//4
        public Array_Structure aTexData { get; set; } //8   8: Array: 0: aTexData//1251090986  {0: Structure: 1036962405: 256}
        public Unk_2236980467 clothData { get; set; } //24   24: Structure: 2236980467: clothData//2464583091
    }

    public struct Unk_1036962405 //3 bytes, Key:4272717794  //COMPONENT PEDS (cloth?) TexData
    {
        public byte texId { get; set; } //0   0: UnsignedByte: 0: texId
        public byte distribution { get; set; } //1   1: UnsignedByte: 0: distribution//914976023
        public byte Unused0 { get; set; }//2
    }

    public struct Unk_2236980467 //24 bytes, Key:508935687  //COMPONENT PEDS clothData
    {
        public byte Unk_2828247905 { get; set; } //0   0: Boolean: 0: 2828247905
        public byte Unused0 { get; set; }//1
        public ushort Unused1 { get; set; }//2
        public uint Unused2 { get; set; }//4
        public uint Unused3 { get; set; }//8
        public uint Unused4 { get; set; }//12
        public uint Unused5 { get; set; }//16
        public uint Unused6 { get; set; }//20
    }

    public struct Unk_253191135 //48 bytes, Key:3120284999  //COMPONENT PEDS unknown
    {
        public MetaHash name { get; set; } //0   0: Hash: 0: name
        public ArrayOfBytes12 Unk_173599222 { get; set; } //4   4: ArrayOfBytes: 12: 173599222
        public ArrayOfBytes12 Unk_2991454271 { get; set; } //16   16: ArrayOfBytes: 12: 2991454271
        public ArrayOfBytes6 Unk_3598106198 { get; set; } //28   28: ArrayOfBytes: 6: 3598106198
        public ArrayOfBytes6 Unk_2095974912 { get; set; } //34   34: ArrayOfBytes: 6: 2095974912
        public ArrayOfBytes6 Unk_672172037 { get; set; } //40   40: ArrayOfBytes: 6: 672172037
        public ushort Unused0 { get; set; }//46
    }

    public struct CComponentInfo //48 bytes, Key:3693847250  //COMPONENT PEDS CComponentInfo
    {
        public MetaHash Unk_802196719 { get; set; } //0   0: Hash: 0: 802196719
        public MetaHash Unk_4233133352 { get; set; } //4   4: Hash: 0: 4233133352
        public ArrayOfBytes5 Unk_128864925 { get; set; } //8   8: ArrayOfBytes: 5: 128864925
        public byte Unused0 { get; set; }//13
        public ushort Unused1 { get; set; }//14
        public uint Unused2 { get; set; }//16
        public uint Unused3 { get; set; }//20
        public uint Unused4 { get; set; }//24
        public uint flags { get; set; } //28   28: UnsignedInt: 0: flags
        public int inclusions { get; set; } //32   32: IntFlags2: 0: inclusions//2172318933
        public int exclusions { get; set; } //36   36: IntFlags2: 0: exclusions
        public Unk_884254308 Unk_1613922652 { get; set; } //40   40: ShortFlags: 884254308: 1613922652
        public ushort Unk_2114993291 { get; set; } //42   42: UnsignedShort: 0: 2114993291
        public byte Unk_3509540765 { get; set; } //44   44: UnsignedByte: 0: 3509540765
        public byte Unk_4196345791 { get; set; } //45   45: UnsignedByte: 0: 4196345791
        public ushort Unused5 { get; set; }//46
    }

    public struct Unk_2858946626 //40 bytes, Key:1792487819  //COMPONENT PEDS unknown
    {
        public byte Unk_2598445407 { get; set; } //0   0: UnsignedByte: 0: 2598445407
        public byte Unused0 { get; set; }//1
        public ushort Unused1 { get; set; }//2
        public uint Unused2 { get; set; }//4
        public Array_Structure Unk_3902803273 { get; set; } //8   8: Array: 0: 3902803273  {0: Structure: 94549140: 256}
        public Array_Structure aAnchors { get; set; } //24   24: Array: 0: aAnchors//162345210  {0: Structure: CAnchorProps//2170383875: 256}
    }

    public struct Unk_94549140 //56 bytes, Key:2029738350  //COMPONENT PEDS unknown
    {
        public MetaHash audioId { get; set; } //0   0: Hash: 0: audioId
        public ArrayOfBytes5 expressionMods { get; set; } //4   4: ArrayOfBytes: 5: expressionMods//942761829
        public byte Unused0 { get; set; }//9
        public ushort Unused1 { get; set; }//10
        public uint Unused2 { get; set; }//12
        public uint Unused3 { get; set; }//16
        public uint Unused4 { get; set; }//20
        public Array_Structure texData { get; set; } //24   24: Array: 0: texData//4088935562  {0: Structure: 254518642: 256}
        public Unk_4212977111 renderFlags { get; set; } //40   40: IntFlags1: 4212977111: renderFlags//4239582912
        public uint propFlags { get; set; } //44   44: UnsignedInt: 0: propFlags//1066841901
        public ushort flags { get; set; } //48   48: UnsignedShort: 0: flags
        public byte anchorId { get; set; } //50   50: UnsignedByte: 0: anchorId//2731224028
        public byte propId { get; set; } //51   51: UnsignedByte: 0: propId//3817142252
        public byte Unk_2894625425 { get; set; } //52   52: UnsignedByte: 0: 2894625425
        public byte Unused5 { get; set; }//53
        public ushort Unused6 { get; set; }//54
    }

    public struct Unk_254518642 //12 bytes, Key:2767296137  //COMPONENT PEDS (expression?) texData
    {
        public int inclusions { get; set; } //0   0: IntFlags2: 0: inclusions//2172318933
        public int exclusions { get; set; } //4   4: IntFlags2: 0: exclusions
        public byte texId { get; set; } //8   8: UnsignedByte: 0: texId
        public byte inclusionId { get; set; } //9   9: UnsignedByte: 0: inclusionId//1938349561
        public byte exclusionId { get; set; } //10   10: UnsignedByte: 0: exclusionId//3819522186
        public byte distribution { get; set; } //11   11: UnsignedByte: 0: distribution//914976023
    }

    public struct CAnchorProps //24 bytes, Key:403574180  //COMPONENT PEDS CAnchorProps
    {
        public Array_byte props { get; set; } //0   0: Array: 0: props  {0: UnsignedByte: 0: 256}
        public Unk_2834549053 anchor { get; set; } //16   16: IntEnum: 2834549053: anchor
        public uint Unused0 { get; set; }//20
    }










}
