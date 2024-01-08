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
using System.Reflection;
using System.Threading;
using System.Runtime.CompilerServices;
using System.Diagnostics.CodeAnalysis;
using System.Buffers.Binary;
using System.Buffers;
using Collections.Pooled;
using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;
using CommunityToolkit.Diagnostics;
using static System.Runtime.InteropServices.JavaScript.JSType;
using CodeWalker.Core.Utils;

namespace CodeWalker.GameFiles
{

    //this is a helper class for parsing the data.
    [SkipLocalsInit]
    public static class MetaTypes
    {

        public static Dictionary<uint, MetaEnumInfo> EnumDict = new Dictionary<uint, MetaEnumInfo>();
        public static Dictionary<uint, MetaStructureInfo> StructDict = new Dictionary<uint, MetaStructureInfo>();

        public static void Clear()
        {
            EnumDict.Clear();
            StructDict.Clear();
        }

        public static void EnsureMetaTypes(Meta meta)
        {

            if (meta.EnumInfos is not null)
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
                            //throw new Exception("Mismatching MetaEnumInfos!");
                        }
                    }
                }
            }

            if (meta.StructureInfos is not null)
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
                            //throw new Exception("Mismatching MetaStructureInfos!");
                        }
                    }
                }
            }

        }

        public static bool CompareMetaEnumInfos(MetaEnumInfo a, MetaEnumInfo b)
        {
            //returns true if they are the same.

            var aEntries = a.Entries;
            var bEntries = b.Entries;
            if (aEntries.Length != bEntries.Length)
            {
                return false;
            }

            for (int i = 0; i < aEntries.Length; i++)
            {
                if ((aEntries[i].EntryNameHash != bEntries[i].EntryNameHash) ||
                    (aEntries[i].EntryValue != bEntries[i].EntryValue))
                {
                    return false;
                }
            }

            return true;
        }
        public static bool CompareMetaStructureInfos(MetaStructureInfo a, MetaStructureInfo b)
        {
            //returns true if they are the same.

            var aEntries = a.Entries;
            var bEntries = b.Entries;

            if (aEntries.Length != bEntries.Length)
            {
                return false;
            }

            for (int i = 0; i < aEntries.Length; i++)
            {
                if ((aEntries[i].EntryNameHash != bEntries[i].EntryNameHash) ||
                    (aEntries[i].DataOffset != bEntries[i].DataOffset) ||
                    (aEntries[i].DataType != bEntries[i].DataType))
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

                    if ((entry.DataOffset == 0) && (entry.EntryNameHash == (MetaName)MetaTypeName.ARRAYINFO)) //referred to by array
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
                AddStructureInfoString(si, sb);
            }

            sb.AppendLine();

            foreach (var ei in meta.EnumInfos)
            {
                AddEnumInfoString(ei, sb);
            }

            string str = sb.ToString();
            return str;
        }
        public static string GetTypesInitString()
        {
            StringBuilder sb = new StringBuilder();

            foreach (var si in StructDict.Values)
            {
                AddStructureInfoString(si, sb);
            }

            sb.AppendLine();

            foreach (var ei in EnumDict.Values)
            {
                AddEnumInfoString(ei, sb);
            }

            string str = sb.ToString();
            return str;
        }
        private static void AddStructureInfoString(MetaStructureInfo si, StringBuilder sb)
        {
            var ns = GetMetaNameString(si.StructureNameHash);
            sb.AppendFormat("case " + ns + ':');
            sb.AppendLine();
            sb.AppendFormat("return new MetaStructureInfo({0}, {1}, {2}, {3},", ns, si.StructureKey, si.Unknown_8h, si.StructureSize);
            sb.AppendLine();
            for (int i = 0; i < si.Entries.Length; i++)
            {
                var e = si.Entries[i];
                string refkey = "0";
                if (e.ReferenceKey != 0)
                {
                    refkey = GetMetaNameString(e.ReferenceKey);
                }
                sb.AppendFormat(" new MetaStructureEntryInfo_s({0}, {1}, MetaStructureEntryDataType.{2}, {3}, {4}, {5})", GetMetaNameString(e.EntryNameHash), e.DataOffset, e.DataType, e.Unknown_9h, e.ReferenceTypeIndex, refkey);
                if (i < si.Entries.Length - 1) sb.Append(',');
                sb.AppendLine();
            }
            sb.AppendFormat(");");
            sb.AppendLine();
        }
        private static void AddEnumInfoString(MetaEnumInfo ei, StringBuilder sb)
        {
            var ns = GetMetaNameString(ei.EnumNameHash);
            sb.AppendFormat("case " + ns + ':');
            sb.AppendLine();
            sb.AppendFormat("return new MetaEnumInfo({0}, {1},", ns, ei.EnumKey);
            sb.AppendLine();
            for (int i = 0; i < ei.Entries.Length; i++)
            {
                var e = ei.Entries[i];
                sb.AppendFormat(" new MetaEnumEntryInfo_s({0}, {1})", GetMetaNameString(e.EntryNameHash), e.EntryValue);
                if (i < ei.Entries.Length - 1) sb.Append(',');
                sb.AppendLine();
            }
            sb.AppendFormat(");");
            sb.AppendLine();
        }
        private static string GetMetaNameString(MetaName n)
        {
            if (Enum.IsDefined(typeof(MetaName), n))
            {
                return "MetaName." + n.ToString();
            }
            else
            {
                return "(MetaName)" + n.ToString();
            }
        }


        public static MetaStructureInfo? GetStructureInfo(MetaName name)
        {
            //to generate structinfos
            return name switch
            {
                MetaName.CScenarioPointContainer => new MetaStructureInfo(MetaName.CScenarioPointContainer, 2489654897, 768, 48,
                                     new MetaStructureEntryInfo_s((MetaName)MetaTypeName.ARRAYINFO, 0, MetaStructureEntryDataType.Structure, 0, 0, MetaName.CExtensionDefSpawnPoint),
                                     new MetaStructureEntryInfo_s(MetaName.LoadSavePoints, 0, MetaStructureEntryDataType.Array, 0, 0, 0),
                                     new MetaStructureEntryInfo_s((MetaName)MetaTypeName.ARRAYINFO, 0, MetaStructureEntryDataType.Structure, 0, 0, MetaName.CScenarioPoint),
                                     new MetaStructureEntryInfo_s(MetaName.MyPoints, 16, MetaStructureEntryDataType.Array, 0, 2, 0)
                                    ),
                MetaName.CScenarioChainingGraph => new MetaStructureInfo(MetaName.CScenarioChainingGraph, 88255871, 768, 88,
                                     new MetaStructureEntryInfo_s((MetaName)MetaTypeName.ARRAYINFO, 0, MetaStructureEntryDataType.Structure, 0, 0, MetaName.CScenarioChainingNode),
                                     new MetaStructureEntryInfo_s(MetaName.Nodes, 0, MetaStructureEntryDataType.Array, 0, 0, 0),
                                     new MetaStructureEntryInfo_s((MetaName)MetaTypeName.ARRAYINFO, 0, MetaStructureEntryDataType.Structure, 0, 0, MetaName.CScenarioChainingEdge),
                                     new MetaStructureEntryInfo_s(MetaName.Edges, 16, MetaStructureEntryDataType.Array, 0, 2, 0),
                                     new MetaStructureEntryInfo_s((MetaName)MetaTypeName.ARRAYINFO, 0, MetaStructureEntryDataType.Structure, 0, 0, MetaName.CScenarioChain),
                                     new MetaStructureEntryInfo_s(MetaName.Chains, 32, MetaStructureEntryDataType.Array, 0, 4, 0)
                                    ),
                MetaName.rage__spdGrid2D => new MetaStructureInfo(MetaName.rage__spdGrid2D, 894636096, 768, 64,
                                     new MetaStructureEntryInfo_s(MetaName.MinCellX, 12, MetaStructureEntryDataType.SignedInt, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.MaxCellX, 16, MetaStructureEntryDataType.SignedInt, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.MinCellY, 20, MetaStructureEntryDataType.SignedInt, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.MaxCellY, 24, MetaStructureEntryDataType.SignedInt, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.CellDimX, 44, MetaStructureEntryDataType.Float, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.CellDimY, 48, MetaStructureEntryDataType.Float, 0, 0, 0)
                                    ),
                MetaName.CScenarioPointLookUps => new MetaStructureInfo(MetaName.CScenarioPointLookUps, 2669361587, 768, 96,
                                     new MetaStructureEntryInfo_s((MetaName)MetaTypeName.ARRAYINFO, 0, MetaStructureEntryDataType.Hash, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.TypeNames, 0, MetaStructureEntryDataType.Array, 0, 0, 0),
                                     new MetaStructureEntryInfo_s((MetaName)MetaTypeName.ARRAYINFO, 0, MetaStructureEntryDataType.Hash, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.PedModelSetNames, 16, MetaStructureEntryDataType.Array, 0, 2, 0),
                                     new MetaStructureEntryInfo_s((MetaName)MetaTypeName.ARRAYINFO, 0, MetaStructureEntryDataType.Hash, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.VehicleModelSetNames, 32, MetaStructureEntryDataType.Array, 0, 4, 0),
                                     new MetaStructureEntryInfo_s((MetaName)MetaTypeName.ARRAYINFO, 0, MetaStructureEntryDataType.Hash, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.GroupNames, 48, MetaStructureEntryDataType.Array, 0, 6, 0),
                                     new MetaStructureEntryInfo_s((MetaName)MetaTypeName.ARRAYINFO, 0, MetaStructureEntryDataType.Hash, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.InteriorNames, 64, MetaStructureEntryDataType.Array, 0, 8, 0),
                                     new MetaStructureEntryInfo_s((MetaName)MetaTypeName.ARRAYINFO, 0, MetaStructureEntryDataType.Hash, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.RequiredIMapNames, 80, MetaStructureEntryDataType.Array, 0, 10, 0)
                                    ),
                MetaName.CScenarioPointRegion => new MetaStructureInfo(MetaName.CScenarioPointRegion, 3501351821, 768, 376,
                                     new MetaStructureEntryInfo_s(MetaName.VersionNumber, 0, MetaStructureEntryDataType.SignedInt, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.Points, 8, MetaStructureEntryDataType.Structure, 0, 0, MetaName.CScenarioPointContainer),
                                     new MetaStructureEntryInfo_s((MetaName)MetaTypeName.ARRAYINFO, 0, MetaStructureEntryDataType.Structure, 0, 0, MetaName.CScenarioEntityOverride),
                                     new MetaStructureEntryInfo_s(MetaName.EntityOverrides, 72, MetaStructureEntryDataType.Array, 0, 2, 0),
                                     new MetaStructureEntryInfo_s(MetaName.ChainingGraph, 96, MetaStructureEntryDataType.Structure, 0, 0, MetaName.CScenarioChainingGraph),
                                     new MetaStructureEntryInfo_s(MetaName.AccelGrid, 184, MetaStructureEntryDataType.Structure, 0, 0, MetaName.rage__spdGrid2D),
                                     new MetaStructureEntryInfo_s((MetaName)MetaTypeName.ARRAYINFO, 0, MetaStructureEntryDataType.UnsignedShort, 0, 0, 0),
                                     new MetaStructureEntryInfo_s((MetaName)3844724227, 248, MetaStructureEntryDataType.Array, 0, 6, 0),
                                     new MetaStructureEntryInfo_s((MetaName)MetaTypeName.ARRAYINFO, 0, MetaStructureEntryDataType.Structure, 0, 0, MetaName.CScenarioPointCluster),
                                     new MetaStructureEntryInfo_s(MetaName.Clusters, 264, MetaStructureEntryDataType.Array, 0, 8, 0),
                                     new MetaStructureEntryInfo_s(MetaName.LookUps, 280, MetaStructureEntryDataType.Structure, 0, 0, MetaName.CScenarioPointLookUps)
                                    ),
                MetaName.CScenarioPoint => new MetaStructureInfo(MetaName.CScenarioPoint, 402442150, 1024, 64,
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
                                     new MetaStructureEntryInfo_s(MetaName.Flags, 36, MetaStructureEntryDataType.IntFlags2, 0, 32, MetaName.CScenarioPointFlags__Flags),
                                     new MetaStructureEntryInfo_s(MetaName.vPositionAndDirection, 48, MetaStructureEntryDataType.Float_XYZW, 0, 0, 0)
                                    ),
                MetaName.CScenarioEntityOverride => new MetaStructureInfo(MetaName.CScenarioEntityOverride, 1271200492, 1024, 80,
                                     new MetaStructureEntryInfo_s(MetaName.EntityPosition, 0, MetaStructureEntryDataType.Float_XYZ, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.EntityType, 16, MetaStructureEntryDataType.Hash, 0, 0, 0),
                                     new MetaStructureEntryInfo_s((MetaName)MetaTypeName.ARRAYINFO, 0, MetaStructureEntryDataType.Structure, 0, 0, MetaName.CExtensionDefSpawnPoint),
                                     new MetaStructureEntryInfo_s(MetaName.ScenarioPoints, 24, MetaStructureEntryDataType.Array, 0, 2, 0),
                                     new MetaStructureEntryInfo_s(MetaName.EntityMayNotAlwaysExist, 64, MetaStructureEntryDataType.Boolean, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.SpecificallyPreventArtPoints, 65, MetaStructureEntryDataType.Boolean, 0, 0, 0)
                                    ),
                MetaName.CExtensionDefSpawnPoint => new MetaStructureInfo(MetaName.CExtensionDefSpawnPoint, 3077340721, 1024, 96,
                                     new MetaStructureEntryInfo_s(MetaName.name, 8, MetaStructureEntryDataType.Hash, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.offsetPosition, 16, MetaStructureEntryDataType.Float_XYZ, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.offsetRotation, 32, MetaStructureEntryDataType.Float_XYZW, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.spawnType, 48, MetaStructureEntryDataType.Hash, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.pedType, 52, MetaStructureEntryDataType.Hash, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.group, 56, MetaStructureEntryDataType.Hash, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.interior, 60, MetaStructureEntryDataType.Hash, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.requiredImap, 64, MetaStructureEntryDataType.Hash, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.availableInMpSp, 68, MetaStructureEntryDataType.IntEnum, 0, 0, MetaName.CSpawnPoint__AvailabilityMpSp),
                                     new MetaStructureEntryInfo_s(MetaName.probability, 72, MetaStructureEntryDataType.Float, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.timeTillPedLeaves, 76, MetaStructureEntryDataType.Float, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.radius, 80, MetaStructureEntryDataType.Float, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.start, 84, MetaStructureEntryDataType.UnsignedByte, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.end, 85, MetaStructureEntryDataType.UnsignedByte, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.flags, 88, MetaStructureEntryDataType.IntFlags2, 0, 32, MetaName.CScenarioPointFlags__Flags),
                                     new MetaStructureEntryInfo_s(MetaName.highPri, 92, MetaStructureEntryDataType.Boolean, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.extendedRange, 93, MetaStructureEntryDataType.Boolean, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.shortRange, 94, MetaStructureEntryDataType.Boolean, 0, 0, 0)
                                    ),
                MetaName.CScenarioChainingNode => new MetaStructureInfo(MetaName.CScenarioChainingNode, 1811784424, 1024, 32,
                                     new MetaStructureEntryInfo_s(MetaName.Position, 0, MetaStructureEntryDataType.Float_XYZ, 0, 0, 0),
                                     new MetaStructureEntryInfo_s((MetaName)2602393771, 16, MetaStructureEntryDataType.Hash, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.ScenarioType, 20, MetaStructureEntryDataType.Hash, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.HasIncomingEdges, 24, MetaStructureEntryDataType.Boolean, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.HasOutgoingEdges, 25, MetaStructureEntryDataType.Boolean, 0, 0, 0)
                                    ),
                MetaName.CScenarioChainingEdge => new MetaStructureInfo(MetaName.CScenarioChainingEdge, 2004985940, 256, 8,
                                     new MetaStructureEntryInfo_s(MetaName.NodeIndexFrom, 0, MetaStructureEntryDataType.UnsignedShort, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.NodeIndexTo, 2, MetaStructureEntryDataType.UnsignedShort, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.Action, 4, MetaStructureEntryDataType.ByteEnum, 0, 0, MetaName.CScenarioChainingEdge__eAction),
                                     new MetaStructureEntryInfo_s(MetaName.NavMode, 5, MetaStructureEntryDataType.ByteEnum, 0, 0, MetaName.CScenarioChainingEdge__eNavMode),
                                     new MetaStructureEntryInfo_s(MetaName.NavSpeed, 6, MetaStructureEntryDataType.ByteEnum, 0, 0, MetaName.CScenarioChainingEdge__eNavSpeed)
                                    ),
                MetaName.CScenarioChain => new MetaStructureInfo(MetaName.CScenarioChain, 2751910366, 768, 40,
                                     new MetaStructureEntryInfo_s((MetaName)1156691834, 0, MetaStructureEntryDataType.UnsignedByte, 0, 0, 0),
                                     new MetaStructureEntryInfo_s((MetaName)MetaTypeName.ARRAYINFO, 0, MetaStructureEntryDataType.UnsignedShort, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.EdgeIds, 8, MetaStructureEntryDataType.Array, 0, 1, 0)
                                    ),
                MetaName.rage__spdSphere => new MetaStructureInfo(MetaName.rage__spdSphere, 1189037266, 1024, 16,
                                     new MetaStructureEntryInfo_s(MetaName.centerAndRadius, 0, MetaStructureEntryDataType.Float_XYZW, 0, 0, 0)
                                    ),
                MetaName.CScenarioPointCluster => new MetaStructureInfo(MetaName.CScenarioPointCluster, 3622480419, 1024, 80,
                                     new MetaStructureEntryInfo_s(MetaName.Points, 0, MetaStructureEntryDataType.Structure, 0, 0, MetaName.CScenarioPointContainer),
                                     new MetaStructureEntryInfo_s(MetaName.ClusterSphere, 48, MetaStructureEntryDataType.Structure, 0, 0, MetaName.rage__spdSphere),
                                     new MetaStructureEntryInfo_s((MetaName)1095875445, 64, MetaStructureEntryDataType.Float, 0, 0, 0),
                                     new MetaStructureEntryInfo_s((MetaName)3129415068, 68, MetaStructureEntryDataType.Boolean, 0, 0, 0)
                                    ),
                MetaName.CStreamingRequestRecord => new MetaStructureInfo(MetaName.CStreamingRequestRecord, 3825587854, 768, 40,
                                     new MetaStructureEntryInfo_s((MetaName)MetaTypeName.ARRAYINFO, 0, MetaStructureEntryDataType.Structure, 0, 0, MetaName.CStreamingRequestFrame),
                                     new MetaStructureEntryInfo_s(MetaName.Frames, 0, MetaStructureEntryDataType.Array, 0, 0, 0),
                                     new MetaStructureEntryInfo_s((MetaName)MetaTypeName.ARRAYINFO, 0, MetaStructureEntryDataType.Structure, 0, 0, MetaName.CStreamingRequestCommonSet),
                                     new MetaStructureEntryInfo_s(MetaName.CommonSets, 16, MetaStructureEntryDataType.Array, 0, 2, 0),
                                     new MetaStructureEntryInfo_s(MetaName.NewStyle, 32, MetaStructureEntryDataType.Boolean, 0, 0, 0)
                                    ),
                MetaName.CStreamingRequestFrame => new MetaStructureInfo(MetaName.CStreamingRequestFrame, 1112444512, 1024, 112,
                                        new MetaStructureEntryInfo_s((MetaName)MetaTypeName.ARRAYINFO, 0, MetaStructureEntryDataType.Hash, 0, 0, 0),
                                        new MetaStructureEntryInfo_s(MetaName.AddList, 0, MetaStructureEntryDataType.Array, 0, 0, 0),
                                        new MetaStructureEntryInfo_s((MetaName)MetaTypeName.ARRAYINFO, 0, MetaStructureEntryDataType.Hash, 0, 0, 0),
                                        new MetaStructureEntryInfo_s(MetaName.RemoveList, 16, MetaStructureEntryDataType.Array, 0, 2, 0),
                                        new MetaStructureEntryInfo_s((MetaName)MetaTypeName.ARRAYINFO, 0, MetaStructureEntryDataType.Hash, 0, 0, 0),
                                        new MetaStructureEntryInfo_s(MetaName.PromoteToHDList, 32, MetaStructureEntryDataType.Array, 0, 4, 0),
                                        new MetaStructureEntryInfo_s(MetaName.CamPos, 48, MetaStructureEntryDataType.Float_XYZ, 0, 0, 0),
                                        new MetaStructureEntryInfo_s(MetaName.CamDir, 64, MetaStructureEntryDataType.Float_XYZ, 0, 0, 0),
                                        new MetaStructureEntryInfo_s((MetaName)MetaTypeName.ARRAYINFO, 0, MetaStructureEntryDataType.UnsignedByte, 0, 0, 0),
                                        new MetaStructureEntryInfo_s(MetaName.CommonAddSets, 80, MetaStructureEntryDataType.Array, 0, 8, 0),
                                        new MetaStructureEntryInfo_s(MetaName.Flags, 96, MetaStructureEntryDataType.UnsignedInt, 0, 0, 0)
                                    ),
                //case MetaName.CStreamingRequestFrame:
                //    return new MetaStructureInfo(MetaName.CStreamingRequestFrame, 3672937465, 1024, 96,
                //     new MetaStructureEntryInfo_s((MetaName)MetaTypeName.ARRAYINFO, 0, MetaStructureEntryDataType.Hash, 0, 0, 0),
                //     new MetaStructureEntryInfo_s(MetaName.AddList, 0, MetaStructureEntryDataType.Array, 0, 0, 0),
                //     new MetaStructureEntryInfo_s((MetaName)MetaTypeName.ARRAYINFO, 0, MetaStructureEntryDataType.Hash, 0, 0, 0),
                //     new MetaStructureEntryInfo_s(MetaName.RemoveList, 16, MetaStructureEntryDataType.Array, 0, 2, 0),
                //     new MetaStructureEntryInfo_s(MetaName.CamPos, 32, MetaStructureEntryDataType.Float_XYZ, 0, 0, 0),
                //     new MetaStructureEntryInfo_s(MetaName.CamDir, 48, MetaStructureEntryDataType.Float_XYZ, 0, 0, 0),
                //     new MetaStructureEntryInfo_s((MetaName)MetaTypeName.ARRAYINFO, 0, MetaStructureEntryDataType.UnsignedByte, 0, 0, 0),
                //     new MetaStructureEntryInfo_s((MetaName)1762439591, 64, MetaStructureEntryDataType.Array, 0, 6, 0),
                //     new MetaStructureEntryInfo_s(MetaName.Flags, 80, MetaStructureEntryDataType.UnsignedInt, 0, 0, 0)
                //    );
                MetaName.CStreamingRequestCommonSet => new MetaStructureInfo(MetaName.CStreamingRequestCommonSet, 3710200606, 768, 16,
                                     new MetaStructureEntryInfo_s((MetaName)MetaTypeName.ARRAYINFO, 0, MetaStructureEntryDataType.Hash, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.Requests, 0, MetaStructureEntryDataType.Array, 0, 0, 0)
                                    ),
                MetaName.CMapTypes => new MetaStructureInfo(MetaName.CMapTypes, 2608875220, 768, 80,
                                     new MetaStructureEntryInfo_s((MetaName)MetaTypeName.ARRAYINFO, 0, MetaStructureEntryDataType.StructurePointer, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.extensions, 8, MetaStructureEntryDataType.Array, 0, 0, 0),
                                     new MetaStructureEntryInfo_s((MetaName)MetaTypeName.ARRAYINFO, 0, MetaStructureEntryDataType.StructurePointer, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.archetypes, 24, MetaStructureEntryDataType.Array, 0, 2, 0),
                                     new MetaStructureEntryInfo_s(MetaName.name, 40, MetaStructureEntryDataType.Hash, 0, 0, 0),
                                     new MetaStructureEntryInfo_s((MetaName)MetaTypeName.ARRAYINFO, 0, MetaStructureEntryDataType.Hash, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.dependencies, 48, MetaStructureEntryDataType.Array, 0, 5, 0),
                                     new MetaStructureEntryInfo_s((MetaName)MetaTypeName.ARRAYINFO, 0, MetaStructureEntryDataType.Structure, 0, 0, MetaName.CCompositeEntityType),
                                     new MetaStructureEntryInfo_s(MetaName.compositeEntityTypes, 64, MetaStructureEntryDataType.Array, 0, 7, 0)
                                    ),
                MetaName.CBaseArchetypeDef => new MetaStructureInfo(MetaName.CBaseArchetypeDef, 2411387556, 1024, 144,
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
                                     new MetaStructureEntryInfo_s(MetaName.assetType, 108, MetaStructureEntryDataType.IntEnum, 0, 0, MetaName.rage__fwArchetypeDef__eAssetType),
                                     new MetaStructureEntryInfo_s(MetaName.assetName, 112, MetaStructureEntryDataType.Hash, 0, 0, 0),
                                     new MetaStructureEntryInfo_s((MetaName)MetaTypeName.ARRAYINFO, 0, MetaStructureEntryDataType.StructurePointer, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.extensions, 120, MetaStructureEntryDataType.Array, 0, 15, 0)
                                    ),
                //case MetaName.CBaseArchetypeDef:
                //    return new MetaStructureInfo(MetaName.CBaseArchetypeDef, 2352343492, 1024, 128,
                //     new MetaStructureEntryInfo_s(MetaName.lodDist, 8, MetaStructureEntryDataType.Float, 0, 0, 0),
                //     new MetaStructureEntryInfo_s(MetaName.flags, 12, MetaStructureEntryDataType.UnsignedInt, 0, 0, 0),
                //     new MetaStructureEntryInfo_s(MetaName.specialAttribute, 16, MetaStructureEntryDataType.UnsignedInt, 0, 0, 0),
                //     new MetaStructureEntryInfo_s(MetaName.bbMin, 32, MetaStructureEntryDataType.Float_XYZ, 0, 0, 0),
                //     new MetaStructureEntryInfo_s(MetaName.bbMax, 48, MetaStructureEntryDataType.Float_XYZ, 0, 0, 0),
                //     new MetaStructureEntryInfo_s(MetaName.bsCentre, 64, MetaStructureEntryDataType.Float_XYZ, 0, 0, 0),
                //     new MetaStructureEntryInfo_s(MetaName.bsRadius, 80, MetaStructureEntryDataType.Float, 0, 0, 0),
                //     new MetaStructureEntryInfo_s(MetaName.hdTextureDist, 84, MetaStructureEntryDataType.Float, 0, 0, 0),
                //     new MetaStructureEntryInfo_s(MetaName.name, 88, MetaStructureEntryDataType.Hash, 0, 0, 0),
                //     new MetaStructureEntryInfo_s(MetaName.textureDictionary, 92, MetaStructureEntryDataType.Hash, 0, 0, 0),
                //     new MetaStructureEntryInfo_s(MetaName.clipDictionary, 96, MetaStructureEntryDataType.Hash, 0, 0, 0),
                //     new MetaStructureEntryInfo_s(MetaName.drawableDictionary, 100, MetaStructureEntryDataType.Hash, 0, 0, 0),
                //     new MetaStructureEntryInfo_s(MetaName.physicsDictionary, 104, MetaStructureEntryDataType.Hash, 0, 0, 0),
                //     new MetaStructureEntryInfo_s((MetaName)MetaTypeName.ARRAYINFO, 0, MetaStructureEntryDataType.StructurePointer, 0, 0, 0),
                //     new MetaStructureEntryInfo_s(MetaName.extensions, 112, MetaStructureEntryDataType.Array, 0, 13, 0)
                //    );
                MetaName.CCreatureMetaData => new MetaStructureInfo(MetaName.CCreatureMetaData, 2181653572, 768, 56,
                                     new MetaStructureEntryInfo_s((MetaName)MetaTypeName.ARRAYINFO, 0, MetaStructureEntryDataType.Structure, 0, 0, MetaName.CShaderVariableComponent),
                                     new MetaStructureEntryInfo_s(MetaName.shaderVariableComponents, 8, MetaStructureEntryDataType.Array, 0, 0, 0),
                                     new MetaStructureEntryInfo_s((MetaName)MetaTypeName.ARRAYINFO, 0, MetaStructureEntryDataType.Structure, 0, 0, MetaName.CPedPropExpressionData),
                                     new MetaStructureEntryInfo_s(MetaName.pedPropExpressions, 24, MetaStructureEntryDataType.Array, 0, 2, 0),
                                     new MetaStructureEntryInfo_s((MetaName)MetaTypeName.ARRAYINFO, 0, MetaStructureEntryDataType.Structure, 0, 0, MetaName.CPedCompExpressionData),
                                     new MetaStructureEntryInfo_s(MetaName.pedCompExpressions, 40, MetaStructureEntryDataType.Array, 0, 4, 0)
                                    ),
                MetaName.CShaderVariableComponent => new MetaStructureInfo(MetaName.CShaderVariableComponent, 3085831725, 768, 72,
                                     new MetaStructureEntryInfo_s(MetaName.pedcompID, 8, MetaStructureEntryDataType.UnsignedInt, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.maskID, 12, MetaStructureEntryDataType.UnsignedInt, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.shaderVariableHashString, 16, MetaStructureEntryDataType.Hash, 0, 0, 0),
                                     new MetaStructureEntryInfo_s((MetaName)MetaTypeName.ARRAYINFO, 0, MetaStructureEntryDataType.UnsignedByte, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.tracks, 24, MetaStructureEntryDataType.Array, 0, 3, 0),
                                     new MetaStructureEntryInfo_s((MetaName)MetaTypeName.ARRAYINFO, 0, MetaStructureEntryDataType.UnsignedShort, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.ids, 40, MetaStructureEntryDataType.Array, 0, 5, 0),
                                     new MetaStructureEntryInfo_s((MetaName)MetaTypeName.ARRAYINFO, 0, MetaStructureEntryDataType.UnsignedByte, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.components, 56, MetaStructureEntryDataType.Array, 0, 7, 0)
                                    ),
                MetaName.CPedPropExpressionData => new MetaStructureInfo(MetaName.CPedPropExpressionData, 1355135810, 768, 88,
                                     new MetaStructureEntryInfo_s(MetaName.pedPropID, 8, MetaStructureEntryDataType.UnsignedInt, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.pedPropVarIndex, 12, MetaStructureEntryDataType.SignedInt, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.pedPropExpressionIndex, 16, MetaStructureEntryDataType.UnsignedInt, 0, 0, 0),
                                     new MetaStructureEntryInfo_s((MetaName)MetaTypeName.ARRAYINFO, 0, MetaStructureEntryDataType.UnsignedByte, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.tracks, 24, MetaStructureEntryDataType.Array, 0, 3, 0),
                                     new MetaStructureEntryInfo_s((MetaName)MetaTypeName.ARRAYINFO, 0, MetaStructureEntryDataType.UnsignedShort, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.ids, 40, MetaStructureEntryDataType.Array, 0, 5, 0),
                                     new MetaStructureEntryInfo_s((MetaName)MetaTypeName.ARRAYINFO, 0, MetaStructureEntryDataType.UnsignedByte, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.types, 56, MetaStructureEntryDataType.Array, 0, 7, 0),
                                     new MetaStructureEntryInfo_s((MetaName)MetaTypeName.ARRAYINFO, 0, MetaStructureEntryDataType.UnsignedByte, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.components, 72, MetaStructureEntryDataType.Array, 0, 9, 0)
                                    ),
                MetaName.CPedCompExpressionData => new MetaStructureInfo(MetaName.CPedCompExpressionData, 3458164745, 768, 88,
                                     new MetaStructureEntryInfo_s(MetaName.pedCompID, 8, MetaStructureEntryDataType.UnsignedInt, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.pedCompVarIndex, 12, MetaStructureEntryDataType.SignedInt, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.pedCompExpressionIndex, 16, MetaStructureEntryDataType.UnsignedInt, 0, 0, 0),
                                     new MetaStructureEntryInfo_s((MetaName)MetaTypeName.ARRAYINFO, 0, MetaStructureEntryDataType.UnsignedByte, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.tracks, 24, MetaStructureEntryDataType.Array, 0, 3, 0),
                                     new MetaStructureEntryInfo_s((MetaName)MetaTypeName.ARRAYINFO, 0, MetaStructureEntryDataType.UnsignedShort, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.ids, 40, MetaStructureEntryDataType.Array, 0, 5, 0),
                                     new MetaStructureEntryInfo_s((MetaName)MetaTypeName.ARRAYINFO, 0, MetaStructureEntryDataType.UnsignedByte, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.types, 56, MetaStructureEntryDataType.Array, 0, 7, 0),
                                     new MetaStructureEntryInfo_s((MetaName)MetaTypeName.ARRAYINFO, 0, MetaStructureEntryDataType.UnsignedByte, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.components, 72, MetaStructureEntryDataType.Array, 0, 9, 0)
                                    ),
                MetaName.rage__fwInstancedMapData => new MetaStructureInfo(MetaName.rage__fwInstancedMapData, 1836780118, 768, 48,
                                     new MetaStructureEntryInfo_s(MetaName.ImapLink, 8, MetaStructureEntryDataType.Hash, 0, 0, 0),
                                     new MetaStructureEntryInfo_s((MetaName)MetaTypeName.ARRAYINFO, 0, MetaStructureEntryDataType.Structure, 0, 0, MetaName.rage__fwPropInstanceListDef),
                                     new MetaStructureEntryInfo_s(MetaName.PropInstanceList, 16, MetaStructureEntryDataType.Array, 0, 1, 0),
                                     new MetaStructureEntryInfo_s((MetaName)MetaTypeName.ARRAYINFO, 0, MetaStructureEntryDataType.Structure, 0, 0, MetaName.rage__fwGrassInstanceListDef),
                                     new MetaStructureEntryInfo_s(MetaName.GrassInstanceList, 32, MetaStructureEntryDataType.Array, 0, 3, 0)
                                    ),
                MetaName.CLODLight => new MetaStructureInfo(MetaName.CLODLight, 2325189228, 768, 136,
                                     new MetaStructureEntryInfo_s((MetaName)MetaTypeName.ARRAYINFO, 0, MetaStructureEntryDataType.Structure, 0, 0, MetaName.FloatXYZ),
                                     new MetaStructureEntryInfo_s(MetaName.direction, 8, MetaStructureEntryDataType.Array, 0, 0, 0),
                                     new MetaStructureEntryInfo_s((MetaName)MetaTypeName.ARRAYINFO, 0, MetaStructureEntryDataType.Float, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.falloff, 24, MetaStructureEntryDataType.Array, 0, 2, 0),
                                     new MetaStructureEntryInfo_s((MetaName)MetaTypeName.ARRAYINFO, 0, MetaStructureEntryDataType.Float, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.falloffExponent, 40, MetaStructureEntryDataType.Array, 0, 4, 0),
                                     new MetaStructureEntryInfo_s((MetaName)MetaTypeName.ARRAYINFO, 0, MetaStructureEntryDataType.UnsignedInt, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.timeAndStateFlags, 56, MetaStructureEntryDataType.Array, 0, 6, 0),
                                     new MetaStructureEntryInfo_s((MetaName)MetaTypeName.ARRAYINFO, 0, MetaStructureEntryDataType.UnsignedInt, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.hash, 72, MetaStructureEntryDataType.Array, 0, 8, 0),
                                     new MetaStructureEntryInfo_s((MetaName)MetaTypeName.ARRAYINFO, 0, MetaStructureEntryDataType.UnsignedByte, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.coneInnerAngle, 88, MetaStructureEntryDataType.Array, 0, 10, 0),
                                     new MetaStructureEntryInfo_s((MetaName)MetaTypeName.ARRAYINFO, 0, MetaStructureEntryDataType.UnsignedByte, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.coneOuterAngleOrCapExt, 104, MetaStructureEntryDataType.Array, 0, 12, 0),
                                     new MetaStructureEntryInfo_s((MetaName)MetaTypeName.ARRAYINFO, 0, MetaStructureEntryDataType.UnsignedByte, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.coronaIntensity, 120, MetaStructureEntryDataType.Array, 0, 14, 0)
                                    ),
                MetaName.CDistantLODLight => new MetaStructureInfo(MetaName.CDistantLODLight, 2820908419, 768, 48,
                                     new MetaStructureEntryInfo_s((MetaName)MetaTypeName.ARRAYINFO, 0, MetaStructureEntryDataType.Structure, 0, 0, MetaName.FloatXYZ),
                                     new MetaStructureEntryInfo_s(MetaName.position, 8, MetaStructureEntryDataType.Array, 0, 0, 0),
                                     new MetaStructureEntryInfo_s((MetaName)MetaTypeName.ARRAYINFO, 0, MetaStructureEntryDataType.UnsignedInt, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.RGBI, 24, MetaStructureEntryDataType.Array, 0, 2, 0),
                                     new MetaStructureEntryInfo_s(MetaName.numStreetLights, 40, MetaStructureEntryDataType.UnsignedShort, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.category, 42, MetaStructureEntryDataType.UnsignedShort, 0, 0, 0)
                                    ),
                MetaName.CBlockDesc => new MetaStructureInfo(MetaName.CBlockDesc, 2015795449, 768, 72,
                                     new MetaStructureEntryInfo_s(MetaName.version, 0, MetaStructureEntryDataType.UnsignedInt, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.flags, 4, MetaStructureEntryDataType.UnsignedInt, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.name, 8, MetaStructureEntryDataType.CharPointer, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.exportedBy, 24, MetaStructureEntryDataType.CharPointer, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.owner, 40, MetaStructureEntryDataType.CharPointer, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.time, 56, MetaStructureEntryDataType.CharPointer, 0, 0, 0)
                                    ),
                MetaName.CMapData => new MetaStructureInfo(MetaName.CMapData, 3448101671, 1024, 512,
                                     new MetaStructureEntryInfo_s(MetaName.name, 8, MetaStructureEntryDataType.Hash, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.parent, 12, MetaStructureEntryDataType.Hash, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.flags, 16, MetaStructureEntryDataType.UnsignedInt, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.contentFlags, 20, MetaStructureEntryDataType.UnsignedInt, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.streamingExtentsMin, 32, MetaStructureEntryDataType.Float_XYZ, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.streamingExtentsMax, 48, MetaStructureEntryDataType.Float_XYZ, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.entitiesExtentsMin, 64, MetaStructureEntryDataType.Float_XYZ, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.entitiesExtentsMax, 80, MetaStructureEntryDataType.Float_XYZ, 0, 0, 0),
                                     new MetaStructureEntryInfo_s((MetaName)MetaTypeName.ARRAYINFO, 0, MetaStructureEntryDataType.StructurePointer, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.entities, 96, MetaStructureEntryDataType.Array, 0, 8, 0),
                                     new MetaStructureEntryInfo_s((MetaName)MetaTypeName.ARRAYINFO, 0, MetaStructureEntryDataType.Structure, 0, 0, MetaName.rage__fwContainerLodDef),
                                     new MetaStructureEntryInfo_s(MetaName.containerLods, 112, MetaStructureEntryDataType.Array, 0, 10, 0),
                                     new MetaStructureEntryInfo_s((MetaName)MetaTypeName.ARRAYINFO, 0, MetaStructureEntryDataType.Structure, 0, 0, MetaName.BoxOccluder),
                                     new MetaStructureEntryInfo_s(MetaName.boxOccluders, 128, MetaStructureEntryDataType.Array, 4, 12, 0),
                                     new MetaStructureEntryInfo_s((MetaName)MetaTypeName.ARRAYINFO, 0, MetaStructureEntryDataType.Structure, 0, 0, MetaName.OccludeModel),
                                     new MetaStructureEntryInfo_s(MetaName.occludeModels, 144, MetaStructureEntryDataType.Array, 4, 14, 0),
                                     new MetaStructureEntryInfo_s((MetaName)MetaTypeName.ARRAYINFO, 0, MetaStructureEntryDataType.Hash, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.physicsDictionaries, 160, MetaStructureEntryDataType.Array, 0, 16, 0),
                                     new MetaStructureEntryInfo_s(MetaName.instancedData, 176, MetaStructureEntryDataType.Structure, 0, 0, MetaName.rage__fwInstancedMapData),
                                     new MetaStructureEntryInfo_s((MetaName)MetaTypeName.ARRAYINFO, 0, MetaStructureEntryDataType.Structure, 0, 0, MetaName.CTimeCycleModifier),
                                     new MetaStructureEntryInfo_s(MetaName.timeCycleModifiers, 224, MetaStructureEntryDataType.Array, 0, 19, 0),
                                     new MetaStructureEntryInfo_s((MetaName)MetaTypeName.ARRAYINFO, 0, MetaStructureEntryDataType.Structure, 0, 0, MetaName.CCarGen),
                                     new MetaStructureEntryInfo_s(MetaName.carGenerators, 240, MetaStructureEntryDataType.Array, 0, 21, 0),
                                     new MetaStructureEntryInfo_s(MetaName.LODLightsSOA, 256, MetaStructureEntryDataType.Structure, 0, 0, MetaName.CLODLight),
                                     new MetaStructureEntryInfo_s(MetaName.DistantLODLightsSOA, 392, MetaStructureEntryDataType.Structure, 0, 0, MetaName.CDistantLODLight),
                                     new MetaStructureEntryInfo_s(MetaName.block, 440, MetaStructureEntryDataType.Structure, 0, 0, MetaName.CBlockDesc)
                                    ),
                MetaName.CEntityDef => new MetaStructureInfo(MetaName.CEntityDef, 1825799514, 1024, 128,
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
                                     new MetaStructureEntryInfo_s(MetaName.lodLevel, 84, MetaStructureEntryDataType.IntEnum, 0, 0, MetaName.rage__eLodType),
                                     new MetaStructureEntryInfo_s(MetaName.numChildren, 88, MetaStructureEntryDataType.UnsignedInt, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.priorityLevel, 92, MetaStructureEntryDataType.IntEnum, 0, 0, MetaName.rage__ePriorityLevel),
                                     new MetaStructureEntryInfo_s((MetaName)MetaTypeName.ARRAYINFO, 0, MetaStructureEntryDataType.StructurePointer, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.extensions, 96, MetaStructureEntryDataType.Array, 0, 13, 0),
                                     new MetaStructureEntryInfo_s(MetaName.ambientOcclusionMultiplier, 112, MetaStructureEntryDataType.SignedInt, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.artificialAmbientOcclusion, 116, MetaStructureEntryDataType.SignedInt, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.tintValue, 120, MetaStructureEntryDataType.UnsignedInt, 0, 0, 0)
                                    ),
                MetaName.CTimeCycleModifier => new MetaStructureInfo(MetaName.CTimeCycleModifier, 2683420777, 1024, 64,
                                     new MetaStructureEntryInfo_s(MetaName.name, 8, MetaStructureEntryDataType.Hash, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.minExtents, 16, MetaStructureEntryDataType.Float_XYZ, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.maxExtents, 32, MetaStructureEntryDataType.Float_XYZ, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.percentage, 48, MetaStructureEntryDataType.Float, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.range, 52, MetaStructureEntryDataType.Float, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.startHour, 56, MetaStructureEntryDataType.UnsignedInt, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.endHour, 60, MetaStructureEntryDataType.UnsignedInt, 0, 0, 0)
                                    ),
                MetaName.CTimeArchetypeDef => new MetaStructureInfo(MetaName.CTimeArchetypeDef, 2520619910, 1024, 160,
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
                                     new MetaStructureEntryInfo_s(MetaName.assetType, 108, MetaStructureEntryDataType.IntEnum, 0, 0, MetaName.rage__fwArchetypeDef__eAssetType),
                                     new MetaStructureEntryInfo_s(MetaName.assetName, 112, MetaStructureEntryDataType.Hash, 0, 0, 0),
                                     new MetaStructureEntryInfo_s((MetaName)MetaTypeName.ARRAYINFO, 0, MetaStructureEntryDataType.StructurePointer, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.extensions, 120, MetaStructureEntryDataType.Array, 0, 15, 0),
                                     new MetaStructureEntryInfo_s(MetaName.timeFlags, 144, MetaStructureEntryDataType.UnsignedInt, 0, 0, 0)
                                    ),
                MetaName.CExtensionDefLightEffect => new MetaStructureInfo(MetaName.CExtensionDefLightEffect, 2436199897, 1024, 48,
                                     new MetaStructureEntryInfo_s(MetaName.name, 8, MetaStructureEntryDataType.Hash, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.offsetPosition, 16, MetaStructureEntryDataType.Float_XYZ, 0, 0, 0),
                                     new MetaStructureEntryInfo_s((MetaName)MetaTypeName.ARRAYINFO, 0, MetaStructureEntryDataType.Structure, 0, 0, MetaName.CLightAttrDef),
                                     new MetaStructureEntryInfo_s(MetaName.instances, 32, MetaStructureEntryDataType.Array, 0, 2, 0)
                                    ),
                MetaName.CLightAttrDef => new MetaStructureInfo(MetaName.CLightAttrDef, 2363260268, 768, 160,
                                     new MetaStructureEntryInfo_s((MetaName)MetaTypeName.ARRAYINFO, 0, MetaStructureEntryDataType.Float, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.posn, 8, MetaStructureEntryDataType.ArrayOfBytes, 0, 0, (MetaName)3),
                                     new MetaStructureEntryInfo_s((MetaName)MetaTypeName.ARRAYINFO, 0, MetaStructureEntryDataType.UnsignedByte, 0, 0, 0),
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
                                     new MetaStructureEntryInfo_s((MetaName)MetaTypeName.ARRAYINFO, 0, MetaStructureEntryDataType.Float, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.cullingPlane, 48, MetaStructureEntryDataType.ArrayOfBytes, 0, 13, (MetaName)4),
                                     new MetaStructureEntryInfo_s(MetaName.shadowBlur, 64, MetaStructureEntryDataType.UnsignedByte, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.padding1, 65, MetaStructureEntryDataType.UnsignedByte, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.padding2, 66, MetaStructureEntryDataType.SignedShort, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.padding3, 68, MetaStructureEntryDataType.UnsignedInt, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.volIntensity, 72, MetaStructureEntryDataType.Float, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.volSizeScale, 76, MetaStructureEntryDataType.Float, 0, 0, 0),
                                     new MetaStructureEntryInfo_s((MetaName)MetaTypeName.ARRAYINFO, 0, MetaStructureEntryDataType.UnsignedByte, 0, 0, 0),
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
                                     new MetaStructureEntryInfo_s((MetaName)MetaTypeName.ARRAYINFO, 0, MetaStructureEntryDataType.Float, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.direction, 112, MetaStructureEntryDataType.ArrayOfBytes, 0, 34, (MetaName)3),
                                     new MetaStructureEntryInfo_s((MetaName)MetaTypeName.ARRAYINFO, 0, MetaStructureEntryDataType.Float, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.tangent, 124, MetaStructureEntryDataType.ArrayOfBytes, 0, 36, (MetaName)3),
                                     new MetaStructureEntryInfo_s(MetaName.coneInnerAngle, 136, MetaStructureEntryDataType.Float, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.coneOuterAngle, 140, MetaStructureEntryDataType.Float, 0, 0, 0),
                                     new MetaStructureEntryInfo_s((MetaName)MetaTypeName.ARRAYINFO, 0, MetaStructureEntryDataType.Float, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.extents, 144, MetaStructureEntryDataType.ArrayOfBytes, 0, 40, (MetaName)3),
                                     new MetaStructureEntryInfo_s(MetaName.projectedTextureKey, 156, MetaStructureEntryDataType.UnsignedInt, 0, 0, 0)
                                    ),
                MetaName.CMloInstanceDef => new MetaStructureInfo(MetaName.CMloInstanceDef, 2151576752, 1024, 160,
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
                                     new MetaStructureEntryInfo_s(MetaName.lodLevel, 84, MetaStructureEntryDataType.IntEnum, 0, 0, MetaName.rage__eLodType),
                                     new MetaStructureEntryInfo_s(MetaName.numChildren, 88, MetaStructureEntryDataType.UnsignedInt, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.priorityLevel, 92, MetaStructureEntryDataType.IntEnum, 0, 0, MetaName.rage__ePriorityLevel),
                                     new MetaStructureEntryInfo_s((MetaName)MetaTypeName.ARRAYINFO, 0, MetaStructureEntryDataType.StructurePointer, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.extensions, 96, MetaStructureEntryDataType.Array, 0, 13, 0),
                                     new MetaStructureEntryInfo_s(MetaName.ambientOcclusionMultiplier, 112, MetaStructureEntryDataType.SignedInt, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.artificialAmbientOcclusion, 116, MetaStructureEntryDataType.SignedInt, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.tintValue, 120, MetaStructureEntryDataType.UnsignedInt, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.groupId, 128, MetaStructureEntryDataType.UnsignedInt, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.floorId, 132, MetaStructureEntryDataType.UnsignedInt, 0, 0, 0),
                                     new MetaStructureEntryInfo_s((MetaName)MetaTypeName.ARRAYINFO, 0, MetaStructureEntryDataType.Hash, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.defaultEntitySets, 136, MetaStructureEntryDataType.Array, 0, 20, 0),
                                     new MetaStructureEntryInfo_s(MetaName.numExitPortals, 152, MetaStructureEntryDataType.UnsignedInt, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.MLOInstflags, 156, MetaStructureEntryDataType.UnsignedInt, 0, 0, 0)
                                    ),
                MetaName.BoxOccluder => new MetaStructureInfo(MetaName.BoxOccluder, 1831736438, 256, 16,
                                     new MetaStructureEntryInfo_s(MetaName.iCenterX, 0, MetaStructureEntryDataType.SignedShort, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.iCenterY, 2, MetaStructureEntryDataType.SignedShort, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.iCenterZ, 4, MetaStructureEntryDataType.SignedShort, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.iCosZ, 6, MetaStructureEntryDataType.SignedShort, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.iLength, 8, MetaStructureEntryDataType.SignedShort, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.iWidth, 10, MetaStructureEntryDataType.SignedShort, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.iHeight, 12, MetaStructureEntryDataType.SignedShort, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.iSinZ, 14, MetaStructureEntryDataType.SignedShort, 0, 0, 0)
                                    ),
                MetaName.OccludeModel => new MetaStructureInfo(MetaName.OccludeModel, 1172796107, 1024, 64,
                                     new MetaStructureEntryInfo_s(MetaName.bmin, 0, MetaStructureEntryDataType.Float_XYZ, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.bmax, 16, MetaStructureEntryDataType.Float_XYZ, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.dataSize, 32, MetaStructureEntryDataType.UnsignedInt, 0, 0, 0),
                                     new MetaStructureEntryInfo_s((MetaName)MetaTypeName.ARRAYINFO, 0, MetaStructureEntryDataType.UnsignedByte, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.verts, 40, MetaStructureEntryDataType.DataBlockPointer, 4, 3, (MetaName)2),
                                     new MetaStructureEntryInfo_s(MetaName.numVertsInBytes, 48, MetaStructureEntryDataType.UnsignedShort, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.numTris, 50, MetaStructureEntryDataType.UnsignedShort, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.flags, 52, MetaStructureEntryDataType.UnsignedInt, 0, 0, 0)
                                    ),
                MetaName.CMloArchetypeDef => new MetaStructureInfo(MetaName.CMloArchetypeDef, 937664754, 1024, 240,
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
                                     new MetaStructureEntryInfo_s(MetaName.assetType, 108, MetaStructureEntryDataType.IntEnum, 0, 0, MetaName.rage__fwArchetypeDef__eAssetType),
                                     new MetaStructureEntryInfo_s(MetaName.assetName, 112, MetaStructureEntryDataType.Hash, 0, 0, 0),
                                     new MetaStructureEntryInfo_s((MetaName)MetaTypeName.ARRAYINFO, 0, MetaStructureEntryDataType.StructurePointer, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.extensions, 120, MetaStructureEntryDataType.Array, 0, 15, 0),
                                     new MetaStructureEntryInfo_s(MetaName.mloFlags, 144, MetaStructureEntryDataType.UnsignedInt, 0, 0, 0),
                                     new MetaStructureEntryInfo_s((MetaName)MetaTypeName.ARRAYINFO, 0, MetaStructureEntryDataType.StructurePointer, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.entities, 152, MetaStructureEntryDataType.Array, 0, 18, 0),
                                     new MetaStructureEntryInfo_s((MetaName)MetaTypeName.ARRAYINFO, 0, MetaStructureEntryDataType.Structure, 0, 0, MetaName.CMloRoomDef),
                                     new MetaStructureEntryInfo_s(MetaName.rooms, 168, MetaStructureEntryDataType.Array, 0, 20, 0),
                                     new MetaStructureEntryInfo_s((MetaName)MetaTypeName.ARRAYINFO, 0, MetaStructureEntryDataType.Structure, 0, 0, MetaName.CMloPortalDef),
                                     new MetaStructureEntryInfo_s(MetaName.portals, 184, MetaStructureEntryDataType.Array, 0, 22, 0),
                                     new MetaStructureEntryInfo_s((MetaName)MetaTypeName.ARRAYINFO, 0, MetaStructureEntryDataType.Structure, 0, 0, MetaName.CMloEntitySet),
                                     new MetaStructureEntryInfo_s(MetaName.entitySets, 200, MetaStructureEntryDataType.Array, 0, 24, 0),
                                     new MetaStructureEntryInfo_s((MetaName)MetaTypeName.ARRAYINFO, 0, MetaStructureEntryDataType.Structure, 0, 0, MetaName.CMloTimeCycleModifier),
                                     new MetaStructureEntryInfo_s(MetaName.timeCycleModifiers, 216, MetaStructureEntryDataType.Array, 0, 26, 0)
                                    ),
                MetaName.CMloRoomDef => new MetaStructureInfo(MetaName.CMloRoomDef, 3885428245, 1024, 112,
                                     new MetaStructureEntryInfo_s(MetaName.name, 8, MetaStructureEntryDataType.CharPointer, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.bbMin, 32, MetaStructureEntryDataType.Float_XYZ, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.bbMax, 48, MetaStructureEntryDataType.Float_XYZ, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.blend, 64, MetaStructureEntryDataType.Float, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.timecycleName, 68, MetaStructureEntryDataType.Hash, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.secondaryTimecycleName, 72, MetaStructureEntryDataType.Hash, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.flags, 76, MetaStructureEntryDataType.UnsignedInt, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.portalCount, 80, MetaStructureEntryDataType.UnsignedInt, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.floorId, 84, MetaStructureEntryDataType.SignedInt, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.exteriorVisibiltyDepth, 88, MetaStructureEntryDataType.SignedInt, 0, 0, 0),
                                     new MetaStructureEntryInfo_s((MetaName)MetaTypeName.ARRAYINFO, 0, MetaStructureEntryDataType.UnsignedInt, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.attachedObjects, 96, MetaStructureEntryDataType.Array, 0, 10, 0)
                                    ),
                MetaName.CMloPortalDef => new MetaStructureInfo(MetaName.CMloPortalDef, 1110221513, 768, 64,
                                     new MetaStructureEntryInfo_s(MetaName.roomFrom, 8, MetaStructureEntryDataType.UnsignedInt, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.roomTo, 12, MetaStructureEntryDataType.UnsignedInt, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.flags, 16, MetaStructureEntryDataType.UnsignedInt, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.mirrorPriority, 20, MetaStructureEntryDataType.UnsignedInt, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.opacity, 24, MetaStructureEntryDataType.UnsignedInt, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.audioOcclusion, 28, MetaStructureEntryDataType.UnsignedInt, 0, 0, 0),
                                     new MetaStructureEntryInfo_s((MetaName)MetaTypeName.ARRAYINFO, 0, MetaStructureEntryDataType.Float_XYZ, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.corners, 32, MetaStructureEntryDataType.Array, 0, 6, 0),
                                     new MetaStructureEntryInfo_s((MetaName)MetaTypeName.ARRAYINFO, 0, MetaStructureEntryDataType.UnsignedInt, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.attachedObjects, 48, MetaStructureEntryDataType.Array, 0, 8, 0)
                                    ),
                MetaName.CMloTimeCycleModifier => new MetaStructureInfo(MetaName.CMloTimeCycleModifier, 838874674, 1024, 48,
                                     new MetaStructureEntryInfo_s(MetaName.name, 8, MetaStructureEntryDataType.Hash, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.sphere, 16, MetaStructureEntryDataType.Float_XYZW, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.percentage, 32, MetaStructureEntryDataType.Float, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.range, 36, MetaStructureEntryDataType.Float, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.startHour, 40, MetaStructureEntryDataType.UnsignedInt, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.endHour, 44, MetaStructureEntryDataType.UnsignedInt, 0, 0, 0)
                                    ),
                MetaName.CExtensionDefParticleEffect => new MetaStructureInfo(MetaName.CExtensionDefParticleEffect, 466596385, 1024, 96,
                                     new MetaStructureEntryInfo_s(MetaName.name, 8, MetaStructureEntryDataType.Hash, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.offsetPosition, 16, MetaStructureEntryDataType.Float_XYZ, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.offsetRotation, 32, MetaStructureEntryDataType.Float_XYZW, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.fxName, 48, MetaStructureEntryDataType.CharPointer, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.fxType, 64, MetaStructureEntryDataType.SignedInt, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.boneTag, 68, MetaStructureEntryDataType.SignedInt, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.scale, 72, MetaStructureEntryDataType.Float, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.probability, 76, MetaStructureEntryDataType.SignedInt, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.flags, 80, MetaStructureEntryDataType.SignedInt, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.color, 84, MetaStructureEntryDataType.UnsignedInt, 0, 0, 0)
                                    ),
                MetaName.CCompositeEntityType => new MetaStructureInfo(MetaName.CCompositeEntityType, 659539004, 1024, 304,
                                     new MetaStructureEntryInfo_s(MetaName.Name, 0, MetaStructureEntryDataType.ArrayOfChars, 0, 0, (MetaName)64),
                                     new MetaStructureEntryInfo_s(MetaName.lodDist, 64, MetaStructureEntryDataType.Float, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.flags, 68, MetaStructureEntryDataType.UnsignedInt, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.specialAttribute, 72, MetaStructureEntryDataType.UnsignedInt, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.bbMin, 80, MetaStructureEntryDataType.Float_XYZ, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.bbMax, 96, MetaStructureEntryDataType.Float_XYZ, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.bsCentre, 112, MetaStructureEntryDataType.Float_XYZ, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.bsRadius, 128, MetaStructureEntryDataType.Float, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.StartModel, 136, MetaStructureEntryDataType.ArrayOfChars, 0, 0, (MetaName)64),
                                     new MetaStructureEntryInfo_s(MetaName.EndModel, 200, MetaStructureEntryDataType.ArrayOfChars, 0, 0, (MetaName)64),
                                     new MetaStructureEntryInfo_s(MetaName.StartImapFile, 264, MetaStructureEntryDataType.Hash, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.EndImapFile, 268, MetaStructureEntryDataType.Hash, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.PtFxAssetName, 272, MetaStructureEntryDataType.Hash, 0, 0, 0),
                                     new MetaStructureEntryInfo_s((MetaName)MetaTypeName.ARRAYINFO, 0, MetaStructureEntryDataType.Structure, 0, 0, MetaName.CCompEntityAnims),
                                     new MetaStructureEntryInfo_s(MetaName.Animations, 280, MetaStructureEntryDataType.Array, 0, 13, 0)
                                    ),
                MetaName.CCompEntityAnims => new MetaStructureInfo(MetaName.CCompEntityAnims, 4110496011, 768, 216,
                                     new MetaStructureEntryInfo_s(MetaName.AnimDict, 0, MetaStructureEntryDataType.ArrayOfChars, 0, 0, (MetaName)64),
                                     new MetaStructureEntryInfo_s(MetaName.AnimName, 64, MetaStructureEntryDataType.ArrayOfChars, 0, 0, (MetaName)64),
                                     new MetaStructureEntryInfo_s(MetaName.AnimatedModel, 128, MetaStructureEntryDataType.ArrayOfChars, 0, 0, (MetaName)64),
                                     new MetaStructureEntryInfo_s(MetaName.punchInPhase, 192, MetaStructureEntryDataType.Float, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.punchOutPhase, 196, MetaStructureEntryDataType.Float, 0, 0, 0),
                                     new MetaStructureEntryInfo_s((MetaName)MetaTypeName.ARRAYINFO, 0, MetaStructureEntryDataType.Structure, 0, 0, MetaName.CCompEntityEffectsData),
                                     new MetaStructureEntryInfo_s(MetaName.effectsData, 200, MetaStructureEntryDataType.Array, 0, 5, 0)
                                    ),
                MetaName.CCompEntityEffectsData => new MetaStructureInfo(MetaName.CCompEntityEffectsData, 1724963966, 1024, 160,
                                     new MetaStructureEntryInfo_s(MetaName.fxType, 0, MetaStructureEntryDataType.UnsignedInt, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.fxOffsetPos, 16, MetaStructureEntryDataType.Float_XYZ, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.fxOffsetRot, 32, MetaStructureEntryDataType.Float_XYZW, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.boneTag, 48, MetaStructureEntryDataType.UnsignedInt, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.startPhase, 52, MetaStructureEntryDataType.Float, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.endPhase, 56, MetaStructureEntryDataType.Float, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.ptFxIsTriggered, 60, MetaStructureEntryDataType.Boolean, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.ptFxTag, 61, MetaStructureEntryDataType.ArrayOfChars, 0, 0, (MetaName)64),
                                     new MetaStructureEntryInfo_s(MetaName.ptFxScale, 128, MetaStructureEntryDataType.Float, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.ptFxProbability, 132, MetaStructureEntryDataType.Float, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.ptFxHasTint, 136, MetaStructureEntryDataType.Boolean, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.ptFxTintR, 137, MetaStructureEntryDataType.UnsignedByte, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.ptFxTintG, 138, MetaStructureEntryDataType.UnsignedByte, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.ptFxTintB, 139, MetaStructureEntryDataType.UnsignedByte, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.ptFxSize, 144, MetaStructureEntryDataType.Float_XYZ, 0, 0, 0)
                                    ),
                MetaName.CExtensionDefAudioCollisionSettings => new MetaStructureInfo(MetaName.CExtensionDefAudioCollisionSettings, 2701897500, 1024, 48,
                                     new MetaStructureEntryInfo_s(MetaName.name, 8, MetaStructureEntryDataType.Hash, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.offsetPosition, 16, MetaStructureEntryDataType.Float_XYZ, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.settings, 32, MetaStructureEntryDataType.Hash, 0, 0, 0)
                                    ),
                MetaName.CExtensionDefAudioEmitter => new MetaStructureInfo(MetaName.CExtensionDefAudioEmitter, 15929839, 1024, 64,
                                     new MetaStructureEntryInfo_s(MetaName.name, 8, MetaStructureEntryDataType.Hash, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.offsetPosition, 16, MetaStructureEntryDataType.Float_XYZ, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.offsetRotation, 32, MetaStructureEntryDataType.Float_XYZW, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.effectHash, 48, MetaStructureEntryDataType.UnsignedInt, 0, 0, 0)
                                    ),
                MetaName.CExtensionDefExplosionEffect => new MetaStructureInfo(MetaName.CExtensionDefExplosionEffect, 2840366784, 1024, 80,
                                     new MetaStructureEntryInfo_s(MetaName.name, 8, MetaStructureEntryDataType.Hash, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.offsetPosition, 16, MetaStructureEntryDataType.Float_XYZ, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.offsetRotation, 32, MetaStructureEntryDataType.Float_XYZW, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.explosionName, 48, MetaStructureEntryDataType.CharPointer, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.boneTag, 64, MetaStructureEntryDataType.SignedInt, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.explosionTag, 68, MetaStructureEntryDataType.SignedInt, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.explosionType, 72, MetaStructureEntryDataType.SignedInt, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.flags, 76, MetaStructureEntryDataType.UnsignedInt, 0, 0, 0)
                                    ),
                MetaName.CExtensionDefLadder => new MetaStructureInfo(MetaName.CExtensionDefLadder, 1978210597, 1024, 96,
                                     new MetaStructureEntryInfo_s(MetaName.name, 8, MetaStructureEntryDataType.Hash, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.offsetPosition, 16, MetaStructureEntryDataType.Float_XYZ, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.bottom, 32, MetaStructureEntryDataType.Float_XYZ, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.top, 48, MetaStructureEntryDataType.Float_XYZ, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.normal, 64, MetaStructureEntryDataType.Float_XYZ, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.materialType, 80, MetaStructureEntryDataType.IntEnum, 0, 0, MetaName.CExtensionDefLadderMaterialType),
                                     new MetaStructureEntryInfo_s(MetaName.template, 84, MetaStructureEntryDataType.Hash, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.canGetOffAtTop, 88, MetaStructureEntryDataType.Boolean, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.canGetOffAtBottom, 89, MetaStructureEntryDataType.Boolean, 0, 0, 0)
                                    ),
                MetaName.CExtensionDefBuoyancy => new MetaStructureInfo(MetaName.CExtensionDefBuoyancy, 2383039928, 1024, 32,
                                     new MetaStructureEntryInfo_s(MetaName.name, 8, MetaStructureEntryDataType.Hash, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.offsetPosition, 16, MetaStructureEntryDataType.Float_XYZ, 0, 0, 0)
                                    ),
                MetaName.CExtensionDefExpression => new MetaStructureInfo(MetaName.CExtensionDefExpression, 24441706, 1024, 48,
                                     new MetaStructureEntryInfo_s(MetaName.name, 8, MetaStructureEntryDataType.Hash, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.offsetPosition, 16, MetaStructureEntryDataType.Float_XYZ, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.expressionDictionaryName, 32, MetaStructureEntryDataType.Hash, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.expressionName, 36, MetaStructureEntryDataType.Hash, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.creatureMetadataName, 40, MetaStructureEntryDataType.Hash, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.initialiseOnCollision, 44, MetaStructureEntryDataType.Boolean, 0, 0, 0)
                                    ),
                MetaName.CExtensionDefLightShaft => new MetaStructureInfo(MetaName.CExtensionDefLightShaft, 2526429398, 1024, 176,
                                     new MetaStructureEntryInfo_s(MetaName.name, 8, MetaStructureEntryDataType.Hash, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.offsetPosition, 16, MetaStructureEntryDataType.Float_XYZ, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.cornerA, 32, MetaStructureEntryDataType.Float_XYZ, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.cornerB, 48, MetaStructureEntryDataType.Float_XYZ, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.cornerC, 64, MetaStructureEntryDataType.Float_XYZ, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.cornerD, 80, MetaStructureEntryDataType.Float_XYZ, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.direction, 96, MetaStructureEntryDataType.Float_XYZ, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.directionAmount, 112, MetaStructureEntryDataType.Float, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.length, 116, MetaStructureEntryDataType.Float, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.fadeInTimeStart, 120, MetaStructureEntryDataType.Float, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.fadeInTimeEnd, 124, MetaStructureEntryDataType.Float, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.fadeOutTimeStart, 128, MetaStructureEntryDataType.Float, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.fadeOutTimeEnd, 132, MetaStructureEntryDataType.Float, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.fadeDistanceStart, 136, MetaStructureEntryDataType.Float, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.fadeDistanceEnd, 140, MetaStructureEntryDataType.Float, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.color, 144, MetaStructureEntryDataType.UnsignedInt, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.intensity, 148, MetaStructureEntryDataType.Float, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.flashiness, 152, MetaStructureEntryDataType.UnsignedByte, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.flags, 156, MetaStructureEntryDataType.UnsignedInt, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.densityType, 160, MetaStructureEntryDataType.IntEnum, 0, 0, MetaName.CExtensionDefLightShaftDensityType),
                                     new MetaStructureEntryInfo_s(MetaName.volumeType, 164, MetaStructureEntryDataType.IntEnum, 0, 0, MetaName.CExtensionDefLightShaftVolumeType),
                                     new MetaStructureEntryInfo_s(MetaName.softness, 168, MetaStructureEntryDataType.Float, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.scaleBySunIntensity, 172, MetaStructureEntryDataType.Boolean, 0, 0, 0)
                                    ),
                MetaName.FloatXYZ => new MetaStructureInfo(MetaName.FloatXYZ, 2751397072, 512, 12,
                                     new MetaStructureEntryInfo_s(MetaName.x, 0, MetaStructureEntryDataType.Float, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.y, 4, MetaStructureEntryDataType.Float, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.z, 8, MetaStructureEntryDataType.Float, 0, 0, 0)
                                    ),
                MetaName.CPedPropInfo => new MetaStructureInfo(MetaName.CPedPropInfo, 1792487819, 768, 40,
                                     new MetaStructureEntryInfo_s(MetaName.numAvailProps, 0, MetaStructureEntryDataType.UnsignedByte, 0, 0, 0),
                                     new MetaStructureEntryInfo_s((MetaName)MetaTypeName.ARRAYINFO, 0, MetaStructureEntryDataType.Structure, 0, 0, MetaName.CPedPropMetaData),
                                     new MetaStructureEntryInfo_s(MetaName.aPropMetaData, 8, MetaStructureEntryDataType.Array, 0, 1, 0),
                                     new MetaStructureEntryInfo_s((MetaName)MetaTypeName.ARRAYINFO, 0, MetaStructureEntryDataType.Structure, 0, 0, MetaName.CAnchorProps),
                                     new MetaStructureEntryInfo_s(MetaName.aAnchors, 24, MetaStructureEntryDataType.Array, 0, 3, 0)
                                    ),
                MetaName.CPedVariationInfo => new MetaStructureInfo(MetaName.CPedVariationInfo, 4030871161, 768, 112,
                                     new MetaStructureEntryInfo_s(MetaName.bHasTexVariations, 0, MetaStructureEntryDataType.Boolean, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.bHasDrawblVariations, 1, MetaStructureEntryDataType.Boolean, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.bHasLowLODs, 2, MetaStructureEntryDataType.Boolean, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.bIsSuperLOD, 3, MetaStructureEntryDataType.Boolean, 0, 0, 0),
                                     new MetaStructureEntryInfo_s((MetaName)MetaTypeName.ARRAYINFO, 0, MetaStructureEntryDataType.UnsignedByte, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.availComp, 4, MetaStructureEntryDataType.ArrayOfBytes, 0, 4, (MetaName)MetaTypeName.PsoPOINTER),
                                     new MetaStructureEntryInfo_s((MetaName)MetaTypeName.ARRAYINFO, 0, MetaStructureEntryDataType.Structure, 0, 0, MetaName.CPVComponentData),
                                     new MetaStructureEntryInfo_s(MetaName.aComponentData3, 16, MetaStructureEntryDataType.Array, 0, 6, 0),
                                     new MetaStructureEntryInfo_s((MetaName)MetaTypeName.ARRAYINFO, 0, MetaStructureEntryDataType.Structure, 0, 0, MetaName.CPedSelectionSet),
                                     new MetaStructureEntryInfo_s(MetaName.aSelectionSets, 32, MetaStructureEntryDataType.Array, 0, 8, 0),
                                     new MetaStructureEntryInfo_s((MetaName)MetaTypeName.ARRAYINFO, 0, MetaStructureEntryDataType.Structure, 0, 0, MetaName.CComponentInfo),
                                     new MetaStructureEntryInfo_s(MetaName.compInfos, 48, MetaStructureEntryDataType.Array, 0, 10, 0),
                                     new MetaStructureEntryInfo_s(MetaName.propInfo, 64, MetaStructureEntryDataType.Structure, 0, 0, MetaName.CPedPropInfo),
                                     new MetaStructureEntryInfo_s(MetaName.dlcName, 104, MetaStructureEntryDataType.Hash, 0, 0, 0)
                                    ),
                MetaName.CPVComponentData => new MetaStructureInfo(MetaName.CPVComponentData, 2024084511, 768, 24,
                                     new MetaStructureEntryInfo_s(MetaName.numAvailTex, 0, MetaStructureEntryDataType.UnsignedByte, 0, 0, 0),
                                     new MetaStructureEntryInfo_s((MetaName)MetaTypeName.ARRAYINFO, 0, MetaStructureEntryDataType.Structure, 0, 0, MetaName.CPVDrawblData),
                                     new MetaStructureEntryInfo_s(MetaName.aDrawblData3, 8, MetaStructureEntryDataType.Array, 0, 1, 0)
                                    ),
                MetaName.CPVDrawblData__CPVClothComponentData => new MetaStructureInfo(MetaName.CPVDrawblData__CPVClothComponentData, 508935687, 0, 24,
                                     new MetaStructureEntryInfo_s(MetaName.ownsCloth, 0, MetaStructureEntryDataType.Boolean, 0, 0, 0)
                                    ),
                MetaName.CPVDrawblData => new MetaStructureInfo(MetaName.CPVDrawblData, 124073662, 768, 48,
                                     new MetaStructureEntryInfo_s(MetaName.propMask, 0, MetaStructureEntryDataType.UnsignedByte, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.numAlternatives, 1, MetaStructureEntryDataType.UnsignedByte, 0, 0, 0),
                                     new MetaStructureEntryInfo_s((MetaName)MetaTypeName.ARRAYINFO, 0, MetaStructureEntryDataType.Structure, 0, 0, MetaName.CPVTextureData),
                                     new MetaStructureEntryInfo_s(MetaName.aTexData, 8, MetaStructureEntryDataType.Array, 0, 2, 0),
                                     new MetaStructureEntryInfo_s(MetaName.clothData, 24, MetaStructureEntryDataType.Structure, 0, 0, MetaName.CPVDrawblData__CPVClothComponentData)
                                    ),
                MetaName.CPVTextureData => new MetaStructureInfo(MetaName.CPVTextureData, 4272717794, 0, 3,
                                     new MetaStructureEntryInfo_s(MetaName.texId, 0, MetaStructureEntryDataType.UnsignedByte, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.distribution, 1, MetaStructureEntryDataType.UnsignedByte, 0, 0, 0)
                                    ),
                MetaName.CComponentInfo => new MetaStructureInfo(MetaName.CComponentInfo, 3693847250, 512, 48,
                                    new MetaStructureEntryInfo_s(MetaName.pedXml_audioID, 0, MetaStructureEntryDataType.Hash, 0, 0, 0),
                                    new MetaStructureEntryInfo_s(MetaName.pedXml_audioID2, 4, MetaStructureEntryDataType.Hash, 0, 0, 0),
                                    new MetaStructureEntryInfo_s((MetaName)MetaTypeName.ARRAYINFO, 0, MetaStructureEntryDataType.Float, 0, 0, 0),
                                    new MetaStructureEntryInfo_s(MetaName.pedXml_expressionMods, 8, MetaStructureEntryDataType.ArrayOfBytes, 0, 2, (MetaName)5),
                                    new MetaStructureEntryInfo_s(MetaName.flags, 28, MetaStructureEntryDataType.UnsignedInt, 0, 0, 0),
                                    new MetaStructureEntryInfo_s(MetaName.inclusions, 32, MetaStructureEntryDataType.IntFlags2, 0, 32, 0),
                                    new MetaStructureEntryInfo_s(MetaName.exclusions, 36, MetaStructureEntryDataType.IntFlags2, 0, 32, 0),
                                    new MetaStructureEntryInfo_s(MetaName.pedXml_vfxComps, 40, MetaStructureEntryDataType.ShortFlags, 0, 16, MetaName.ePedVarComp),
                                    new MetaStructureEntryInfo_s(MetaName.pedXml_flags, 42, MetaStructureEntryDataType.UnsignedShort, 0, 0, 0),
                                    new MetaStructureEntryInfo_s(MetaName.pedXml_compIdx, 44, MetaStructureEntryDataType.UnsignedByte, 0, 0, 0),
                                    new MetaStructureEntryInfo_s(MetaName.pedXml_drawblIdx, 45, MetaStructureEntryDataType.UnsignedByte, 0, 0, 0)
                                    ),
                MetaName.CPedPropMetaData => new MetaStructureInfo(MetaName.CPedPropMetaData, 2029738350, 768, 56,
                                     new MetaStructureEntryInfo_s(MetaName.audioId, 0, MetaStructureEntryDataType.Hash, 0, 0, 0),
                                     new MetaStructureEntryInfo_s((MetaName)MetaTypeName.ARRAYINFO, 0, MetaStructureEntryDataType.Float, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.expressionMods, 4, MetaStructureEntryDataType.ArrayOfBytes, 0, 1, (MetaName)5),
                                     new MetaStructureEntryInfo_s((MetaName)MetaTypeName.ARRAYINFO, 0, MetaStructureEntryDataType.Structure, 0, 0, MetaName.CPedPropTexData),
                                     new MetaStructureEntryInfo_s(MetaName.texData, 24, MetaStructureEntryDataType.Array, 0, 3, 0),
                                     new MetaStructureEntryInfo_s(MetaName.renderFlags, 40, MetaStructureEntryDataType.IntFlags1, 0, 3, MetaName.ePropRenderFlags),
                                     new MetaStructureEntryInfo_s(MetaName.propFlags, 44, MetaStructureEntryDataType.UnsignedInt, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.flags, 48, MetaStructureEntryDataType.UnsignedShort, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.anchorId, 50, MetaStructureEntryDataType.UnsignedByte, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.propId, 51, MetaStructureEntryDataType.UnsignedByte, 0, 0, 0),
                                     new MetaStructureEntryInfo_s((MetaName)2894625425, 52, MetaStructureEntryDataType.UnsignedByte, 0, 0, 0)
                                    ),
                MetaName.CPedPropTexData => new MetaStructureInfo(MetaName.CPedPropTexData, 2767296137, 512, 12,
                                     new MetaStructureEntryInfo_s(MetaName.inclusions, 0, MetaStructureEntryDataType.IntFlags2, 0, 32, 0),
                                     new MetaStructureEntryInfo_s(MetaName.exclusions, 4, MetaStructureEntryDataType.IntFlags2, 0, 32, 0),
                                     new MetaStructureEntryInfo_s(MetaName.texId, 8, MetaStructureEntryDataType.UnsignedByte, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.inclusionId, 9, MetaStructureEntryDataType.UnsignedByte, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.exclusionId, 10, MetaStructureEntryDataType.UnsignedByte, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.distribution, 11, MetaStructureEntryDataType.UnsignedByte, 0, 0, 0)
                                    ),
                MetaName.CAnchorProps => new MetaStructureInfo(MetaName.CAnchorProps, 403574180, 768, 24,
                                     new MetaStructureEntryInfo_s((MetaName)MetaTypeName.ARRAYINFO, 0, MetaStructureEntryDataType.UnsignedByte, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.props, 0, MetaStructureEntryDataType.Array, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.anchor, 16, MetaStructureEntryDataType.IntEnum, 0, 0, MetaName.eAnchorPoints)
                                    ),
                MetaName.CPedSelectionSet => new MetaStructureInfo(MetaName.CPedSelectionSet, 3120284999, 512, 48,
                                     new MetaStructureEntryInfo_s(MetaName.name, 0, MetaStructureEntryDataType.Hash, 0, 0, 0),
                                     new MetaStructureEntryInfo_s((MetaName)MetaTypeName.ARRAYINFO, 0, MetaStructureEntryDataType.UnsignedByte, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.compDrawableId, 4, MetaStructureEntryDataType.ArrayOfBytes, 0, 1, (MetaName)MetaTypeName.PsoPOINTER),
                                     new MetaStructureEntryInfo_s((MetaName)MetaTypeName.ARRAYINFO, 0, MetaStructureEntryDataType.UnsignedByte, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.compTexId, 16, MetaStructureEntryDataType.ArrayOfBytes, 0, 3, (MetaName)MetaTypeName.PsoPOINTER),
                                     new MetaStructureEntryInfo_s((MetaName)MetaTypeName.ARRAYINFO, 0, MetaStructureEntryDataType.UnsignedByte, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.propAnchorId, 28, MetaStructureEntryDataType.ArrayOfBytes, 0, 5, (MetaName)6),
                                     new MetaStructureEntryInfo_s((MetaName)MetaTypeName.ARRAYINFO, 0, MetaStructureEntryDataType.UnsignedByte, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.propDrawableId, 34, MetaStructureEntryDataType.ArrayOfBytes, 0, 7, (MetaName)6),
                                     new MetaStructureEntryInfo_s((MetaName)MetaTypeName.ARRAYINFO, 0, MetaStructureEntryDataType.UnsignedByte, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.propTexId, 40, MetaStructureEntryDataType.ArrayOfBytes, 0, 9, (MetaName)6)
                                    ),
                MetaName.CExtensionDefDoor => new MetaStructureInfo(MetaName.CExtensionDefDoor, 2671601385, 1024, 48,
                                     new MetaStructureEntryInfo_s(MetaName.name, 8, MetaStructureEntryDataType.Hash, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.offsetPosition, 16, MetaStructureEntryDataType.Float_XYZ, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.enableLimitAngle, 32, MetaStructureEntryDataType.Boolean, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.startsLocked, 33, MetaStructureEntryDataType.Boolean, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.canBreak, 34, MetaStructureEntryDataType.Boolean, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.limitAngle, 36, MetaStructureEntryDataType.Float, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.doorTargetRatio, 40, MetaStructureEntryDataType.Float, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.audioHash, 44, MetaStructureEntryDataType.Hash, 0, 0, 0)
                                    ),
                MetaName.CMloEntitySet => new MetaStructureInfo(MetaName.CMloEntitySet, 4180211587, 768, 48,
                                     new MetaStructureEntryInfo_s(MetaName.name, 8, MetaStructureEntryDataType.Hash, 0, 0, 0),
                                     new MetaStructureEntryInfo_s((MetaName)MetaTypeName.ARRAYINFO, 0, MetaStructureEntryDataType.UnsignedInt, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.locations, 16, MetaStructureEntryDataType.Array, 0, 1, 0),
                                     new MetaStructureEntryInfo_s((MetaName)MetaTypeName.ARRAYINFO, 0, MetaStructureEntryDataType.StructurePointer, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.entities, 32, MetaStructureEntryDataType.Array, 0, 3, 0)
                                    ),
                MetaName.CExtensionDefSpawnPointOverride => new MetaStructureInfo(MetaName.CExtensionDefSpawnPointOverride, 2551875873, 1024, 64,
                                     new MetaStructureEntryInfo_s(MetaName.name, 8, MetaStructureEntryDataType.Hash, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.offsetPosition, 16, MetaStructureEntryDataType.Float_XYZ, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.ScenarioType, 32, MetaStructureEntryDataType.Hash, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.iTimeStartOverride, 36, MetaStructureEntryDataType.UnsignedByte, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.iTimeEndOverride, 37, MetaStructureEntryDataType.UnsignedByte, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.Group, 40, MetaStructureEntryDataType.Hash, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.ModelSet, 44, MetaStructureEntryDataType.Hash, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.AvailabilityInMpSp, 48, MetaStructureEntryDataType.IntEnum, 0, 0, MetaName.CSpawnPoint__AvailabilityMpSp),
                                     new MetaStructureEntryInfo_s(MetaName.Flags, 52, MetaStructureEntryDataType.IntFlags2, 0, 32, MetaName.CScenarioPointFlags__Flags),
                                     new MetaStructureEntryInfo_s(MetaName.Radius, 56, MetaStructureEntryDataType.Float, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.TimeTillPedLeaves, 60, MetaStructureEntryDataType.Float, 0, 0, 0)
                                    ),
                MetaName.CExtensionDefWindDisturbance => new MetaStructureInfo(MetaName.CExtensionDefWindDisturbance, 3971538917, 1024, 96,
                                     new MetaStructureEntryInfo_s(MetaName.name, 8, MetaStructureEntryDataType.Hash, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.offsetPosition, 16, MetaStructureEntryDataType.Float_XYZ, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.offsetRotation, 32, MetaStructureEntryDataType.Float_XYZW, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.disturbanceType, 48, MetaStructureEntryDataType.SignedInt, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.boneTag, 52, MetaStructureEntryDataType.SignedInt, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.size, 64, MetaStructureEntryDataType.Float_XYZW, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.strength, 80, MetaStructureEntryDataType.Float, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.flags, 84, MetaStructureEntryDataType.SignedInt, 0, 0, 0)
                                    ),
                MetaName.CCarGen => new MetaStructureInfo(MetaName.CCarGen, 2345238261, 1024, 80,
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
                                    ),
                MetaName.rage__spdAABB => new MetaStructureInfo(MetaName.rage__spdAABB, 1158138379, 1024, 32,
                                     new MetaStructureEntryInfo_s(MetaName.min, 0, MetaStructureEntryDataType.Float_XYZW, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.max, 16, MetaStructureEntryDataType.Float_XYZW, 0, 0, 0)
                                    ),
                MetaName.rage__fwGrassInstanceListDef => new MetaStructureInfo(MetaName.rage__fwGrassInstanceListDef, 941808164, 1024, 96,
                                     new MetaStructureEntryInfo_s(MetaName.BatchAABB, 0, MetaStructureEntryDataType.Structure, 0, 0, MetaName.rage__spdAABB),
                                     new MetaStructureEntryInfo_s(MetaName.ScaleRange, 32, MetaStructureEntryDataType.Float_XYZ, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.archetypeName, 48, MetaStructureEntryDataType.Hash, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.lodDist, 52, MetaStructureEntryDataType.UnsignedInt, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.LodFadeStartDist, 56, MetaStructureEntryDataType.Float, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.LodInstFadeRange, 60, MetaStructureEntryDataType.Float, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.OrientToTerrain, 64, MetaStructureEntryDataType.Float, 0, 0, 0),
                                     new MetaStructureEntryInfo_s((MetaName)MetaTypeName.ARRAYINFO, 0, MetaStructureEntryDataType.Structure, 0, 0, MetaName.rage__fwGrassInstanceListDef__InstanceData),
                                     new MetaStructureEntryInfo_s(MetaName.InstanceList, 72, MetaStructureEntryDataType.Array, 36, 7, 0)
                                    ),
                MetaName.rage__fwGrassInstanceListDef__InstanceData => new MetaStructureInfo(MetaName.rage__fwGrassInstanceListDef__InstanceData, 2740378365, 256, 16,
                                     new MetaStructureEntryInfo_s((MetaName)MetaTypeName.ARRAYINFO, 0, MetaStructureEntryDataType.UnsignedShort, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.Position, 0, MetaStructureEntryDataType.ArrayOfBytes, 0, 0, (MetaName)3),
                                     new MetaStructureEntryInfo_s(MetaName.NormalX, 6, MetaStructureEntryDataType.UnsignedByte, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.NormalY, 7, MetaStructureEntryDataType.UnsignedByte, 0, 0, 0),
                                     new MetaStructureEntryInfo_s((MetaName)MetaTypeName.ARRAYINFO, 0, MetaStructureEntryDataType.UnsignedByte, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.Color, 8, MetaStructureEntryDataType.ArrayOfBytes, 0, 4, (MetaName)3),
                                     new MetaStructureEntryInfo_s(MetaName.Scale, 11, MetaStructureEntryDataType.UnsignedByte, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.Ao, 12, MetaStructureEntryDataType.UnsignedByte, 0, 0, 0),
                                     new MetaStructureEntryInfo_s((MetaName)MetaTypeName.ARRAYINFO, 0, MetaStructureEntryDataType.UnsignedByte, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.Pad, 13, MetaStructureEntryDataType.ArrayOfBytes, 0, 8, (MetaName)3)
                                    ),
                MetaName.CExtensionDefProcObject => new MetaStructureInfo(MetaName.CExtensionDefProcObject, 3965391891, 1024, 80,
                                     new MetaStructureEntryInfo_s(MetaName.name, 8, MetaStructureEntryDataType.Hash, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.offsetPosition, 16, MetaStructureEntryDataType.Float_XYZ, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.radiusInner, 32, MetaStructureEntryDataType.Float, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.radiusOuter, 36, MetaStructureEntryDataType.Float, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.spacing, 40, MetaStructureEntryDataType.Float, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.minScale, 44, MetaStructureEntryDataType.Float, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.maxScale, 48, MetaStructureEntryDataType.Float, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.minScaleZ, 52, MetaStructureEntryDataType.Float, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.maxScaleZ, 56, MetaStructureEntryDataType.Float, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.minZOffset, 60, MetaStructureEntryDataType.Float, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.maxZOffset, 64, MetaStructureEntryDataType.Float, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.objectHash, 68, MetaStructureEntryDataType.UnsignedInt, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.flags, 72, MetaStructureEntryDataType.UnsignedInt, 0, 0, 0)
                                    ),
                MetaName.rage__phVerletClothCustomBounds => new MetaStructureInfo(MetaName.rage__phVerletClothCustomBounds, 2075461750, 768, 32,
                                     new MetaStructureEntryInfo_s(MetaName.name, 8, MetaStructureEntryDataType.Hash, 0, 0, 0),
                                     new MetaStructureEntryInfo_s((MetaName)MetaTypeName.ARRAYINFO, 0, MetaStructureEntryDataType.Structure, 0, 0, MetaName.rage__phCapsuleBoundDef),
                                     new MetaStructureEntryInfo_s(MetaName.CollisionData, 16, MetaStructureEntryDataType.Array, 0, 1, 0)
                                    ),
                MetaName.rage__phCapsuleBoundDef => new MetaStructureInfo(MetaName.rage__phCapsuleBoundDef, 2859775340, 1024, 96,
                                     new MetaStructureEntryInfo_s(MetaName.OwnerName, 0, MetaStructureEntryDataType.CharPointer, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.Rotation, 16, MetaStructureEntryDataType.Float_XYZW, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.Position, 32, MetaStructureEntryDataType.Float_XYZ, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.Normal, 48, MetaStructureEntryDataType.Float_XYZ, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.CapsuleRadius, 64, MetaStructureEntryDataType.Float, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.CapsuleLen, 68, MetaStructureEntryDataType.Float, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.CapsuleHalfHeight, 72, MetaStructureEntryDataType.Float, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.CapsuleHalfWidth, 76, MetaStructureEntryDataType.Float, 0, 0, 0),
                                     new MetaStructureEntryInfo_s(MetaName.Flags, 80, MetaStructureEntryDataType.IntFlags2, 0, 32, MetaName.rage__phCapsuleBoundDef__enCollisionBoundDef)
                                    ),
                _ => null,
            };
        }
        public static MetaEnumInfo? GetEnumInfo(MetaName name)
        {
            //to generate enuminfos
            return name switch
            {
                MetaName.CScenarioPointFlags__Flags => new MetaEnumInfo(MetaName.CScenarioPointFlags__Flags, 2814596095,
                                     new MetaEnumEntryInfo_s(MetaName.IgnoreMaxInRange, 0),
                                     new MetaEnumEntryInfo_s(MetaName.NoSpawn, 1),
                                     new MetaEnumEntryInfo_s(MetaName.StationaryReactions, 2),
                                     new MetaEnumEntryInfo_s(MetaName.OnlySpawnInSameInterior, 3),
                                     new MetaEnumEntryInfo_s(MetaName.SpawnedPedIsArrestable, 4),
                                     new MetaEnumEntryInfo_s(MetaName.ActivateVehicleSiren, 5),
                                     new MetaEnumEntryInfo_s(MetaName.AggressiveVehicleDriving, 6),
                                     new MetaEnumEntryInfo_s(MetaName.LandVehicleOnArrival, 7),
                                     new MetaEnumEntryInfo_s(MetaName.IgnoreThreatsIfLosNotClear, 8),
                                     new MetaEnumEntryInfo_s(MetaName.EventsInRadiusTriggerDisputes, 9),
                                     new MetaEnumEntryInfo_s(MetaName.AerialVehiclePoint, 10),
                                     new MetaEnumEntryInfo_s(MetaName.TerritorialScenario, 11),
                                     new MetaEnumEntryInfo_s(MetaName.EndScenarioIfPlayerWithinRadius, 12),
                                     new MetaEnumEntryInfo_s(MetaName.EventsInRadiusTriggerThreatResponse, 13),
                                     new MetaEnumEntryInfo_s(MetaName.TaxiPlaneOnGround, 14),
                                     new MetaEnumEntryInfo_s(MetaName.FlyOffToOblivion, 15),
                                     new MetaEnumEntryInfo_s(MetaName.InWater, 16),
                                     new MetaEnumEntryInfo_s(MetaName.AllowInvestigation, 17),
                                     new MetaEnumEntryInfo_s(MetaName.OpenDoor, 18),
                                     new MetaEnumEntryInfo_s(MetaName.PreciseUseTime, 19),
                                     new MetaEnumEntryInfo_s(MetaName.NoRespawnUntilStreamedOut, 20),
                                     new MetaEnumEntryInfo_s(MetaName.NoVehicleSpawnMaxDistance, 21),
                                     new MetaEnumEntryInfo_s(MetaName.ExtendedRange, 22),
                                     new MetaEnumEntryInfo_s(MetaName.ShortRange, 23),
                                     new MetaEnumEntryInfo_s(MetaName.HighPriority, 24),
                                     new MetaEnumEntryInfo_s(MetaName.IgnoreLoitering, 25),
                                     new MetaEnumEntryInfo_s(MetaName.UseSearchlight, 26),
                                     new MetaEnumEntryInfo_s(MetaName.ResetNoCollisionOnCleanUp, 27),
                                     new MetaEnumEntryInfo_s(MetaName.CheckCrossedArrivalPlane, 28),
                                     new MetaEnumEntryInfo_s(MetaName.UseVehicleFrontForArrival, 29),
                                     new MetaEnumEntryInfo_s(MetaName.IgnoreWeatherRestrictions, 30)
                                    ),
                MetaName.CSpawnPoint__AvailabilityMpSp => new MetaEnumInfo(MetaName.CSpawnPoint__AvailabilityMpSp, 671739257,
                                     new MetaEnumEntryInfo_s(MetaName.kBoth, 0),
                                     new MetaEnumEntryInfo_s(MetaName.kOnlySp, 1),
                                     new MetaEnumEntryInfo_s(MetaName.kOnlyMp, 2)
                                    ),
                MetaName.CScenarioChainingEdge__eAction => new MetaEnumInfo(MetaName.CScenarioChainingEdge__eAction, 3326075799,
                                     new MetaEnumEntryInfo_s(MetaName.Move, 0),
                                     new MetaEnumEntryInfo_s((MetaName)7865678, 1),
                                     new MetaEnumEntryInfo_s(MetaName.MoveFollowMaster, 2)
                                    ),
                MetaName.CScenarioChainingEdge__eNavMode => new MetaEnumInfo(MetaName.CScenarioChainingEdge__eNavMode, 3016128742,
                                     new MetaEnumEntryInfo_s(MetaName.Direct, 0),
                                     new MetaEnumEntryInfo_s(MetaName.NavMesh, 1),
                                     new MetaEnumEntryInfo_s(MetaName.Roads, 2)
                                    ),
                MetaName.CScenarioChainingEdge__eNavSpeed => new MetaEnumInfo(MetaName.CScenarioChainingEdge__eNavSpeed, 1112851290,
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
                                    ),
                MetaName.rage__fwArchetypeDef__eAssetType => new MetaEnumInfo(MetaName.rage__fwArchetypeDef__eAssetType, 1866031916,
                                     new MetaEnumEntryInfo_s(MetaName.ASSET_TYPE_UNINITIALIZED, 0),
                                     new MetaEnumEntryInfo_s(MetaName.ASSET_TYPE_FRAGMENT, 1),
                                     new MetaEnumEntryInfo_s(MetaName.ASSET_TYPE_DRAWABLE, 2),
                                     new MetaEnumEntryInfo_s(MetaName.ASSET_TYPE_DRAWABLEDICTIONARY, 3),
                                     new MetaEnumEntryInfo_s(MetaName.ASSET_TYPE_ASSETLESS, 4)
                                    ),
                MetaName.rage__eLodType => new MetaEnumInfo(MetaName.rage__eLodType, 1856311430,
                                     new MetaEnumEntryInfo_s(MetaName.LODTYPES_DEPTH_HD, 0),
                                     new MetaEnumEntryInfo_s(MetaName.LODTYPES_DEPTH_LOD, 1),
                                     new MetaEnumEntryInfo_s(MetaName.LODTYPES_DEPTH_SLOD1, 2),
                                     new MetaEnumEntryInfo_s(MetaName.LODTYPES_DEPTH_SLOD2, 3),
                                     new MetaEnumEntryInfo_s(MetaName.LODTYPES_DEPTH_SLOD3, 4),
                                     new MetaEnumEntryInfo_s(MetaName.LODTYPES_DEPTH_ORPHANHD, 5),
                                     new MetaEnumEntryInfo_s(MetaName.LODTYPES_DEPTH_SLOD4, 6)
                                    ),
                MetaName.rage__ePriorityLevel => new MetaEnumInfo(MetaName.rage__ePriorityLevel, 2200357711,
                                     new MetaEnumEntryInfo_s(MetaName.PRI_REQUIRED, 0),
                                     new MetaEnumEntryInfo_s(MetaName.PRI_OPTIONAL_HIGH, 1),
                                     new MetaEnumEntryInfo_s(MetaName.PRI_OPTIONAL_MEDIUM, 2),
                                     new MetaEnumEntryInfo_s(MetaName.PRI_OPTIONAL_LOW, 3)
                                    ),
                MetaName.CExtensionDefLadderMaterialType => new MetaEnumInfo(MetaName.CExtensionDefLadderMaterialType, 3514570158,
                                     new MetaEnumEntryInfo_s(MetaName.METAL_SOLID_LADDER, 0),
                                     new MetaEnumEntryInfo_s(MetaName.METAL_LIGHT_LADDER, 1),
                                     new MetaEnumEntryInfo_s(MetaName.WOODEN_LADDER, 2)
                                    ),
                MetaName.CExtensionDefLightShaftDensityType => new MetaEnumInfo(MetaName.CExtensionDefLightShaftDensityType, 3539601182,
                                     new MetaEnumEntryInfo_s(MetaName.LIGHTSHAFT_DENSITYTYPE_CONSTANT, 0),
                                     new MetaEnumEntryInfo_s(MetaName.LIGHTSHAFT_DENSITYTYPE_SOFT, 1),
                                     new MetaEnumEntryInfo_s(MetaName.LIGHTSHAFT_DENSITYTYPE_SOFT_SHADOW, 2),
                                     new MetaEnumEntryInfo_s(MetaName.LIGHTSHAFT_DENSITYTYPE_SOFT_SHADOW_HD, 3),
                                     new MetaEnumEntryInfo_s(MetaName.LIGHTSHAFT_DENSITYTYPE_LINEAR, 4),
                                     new MetaEnumEntryInfo_s(MetaName.LIGHTSHAFT_DENSITYTYPE_LINEAR_GRADIENT, 5),
                                     new MetaEnumEntryInfo_s(MetaName.LIGHTSHAFT_DENSITYTYPE_QUADRATIC, 6),
                                     new MetaEnumEntryInfo_s(MetaName.LIGHTSHAFT_DENSITYTYPE_QUADRATIC_GRADIENT, 7)
                                    ),
                MetaName.CExtensionDefLightShaftVolumeType => new MetaEnumInfo(MetaName.CExtensionDefLightShaftVolumeType, 4287472345,
                                     new MetaEnumEntryInfo_s(MetaName.LIGHTSHAFT_VOLUMETYPE_SHAFT, 0),
                                     new MetaEnumEntryInfo_s(MetaName.LIGHTSHAFT_VOLUMETYPE_CYLINDER, 1)
                                    ),
                MetaName.ePedVarComp => new MetaEnumInfo(MetaName.ePedVarComp, 3472084374,
                                     new MetaEnumEntryInfo_s(MetaName.PV_COMP_INVALID, -1),
                                     new MetaEnumEntryInfo_s(MetaName.PV_COMP_HEAD, 0),
                                     new MetaEnumEntryInfo_s(MetaName.PV_COMP_BERD, 1),
                                     new MetaEnumEntryInfo_s(MetaName.PV_COMP_HAIR, 2),
                                     new MetaEnumEntryInfo_s(MetaName.PV_COMP_UPPR, 3),
                                     new MetaEnumEntryInfo_s(MetaName.PV_COMP_LOWR, 4),
                                     new MetaEnumEntryInfo_s(MetaName.PV_COMP_HAND, 5),
                                     new MetaEnumEntryInfo_s(MetaName.PV_COMP_FEET, 6),
                                     new MetaEnumEntryInfo_s(MetaName.PV_COMP_TEEF, 7),
                                     new MetaEnumEntryInfo_s(MetaName.PV_COMP_ACCS, 8),
                                     new MetaEnumEntryInfo_s(MetaName.PV_COMP_TASK, 9),
                                     new MetaEnumEntryInfo_s(MetaName.PV_COMP_DECL, 10),
                                     new MetaEnumEntryInfo_s(MetaName.PV_COMP_JBIB, 11),
                                     new MetaEnumEntryInfo_s(MetaName.PV_COMP_MAX, 12)
                                    ),
                MetaName.ePropRenderFlags => new MetaEnumInfo(MetaName.ePropRenderFlags, 1551913633,
                                     new MetaEnumEntryInfo_s(MetaName.PRF_ALPHA, 0),
                                     new MetaEnumEntryInfo_s(MetaName.PRF_DECAL, 1),
                                     new MetaEnumEntryInfo_s(MetaName.PRF_CUTOUT, 2)
                                    ),
                MetaName.eAnchorPoints => new MetaEnumInfo(MetaName.eAnchorPoints, 1309372691,
                                     new MetaEnumEntryInfo_s(MetaName.ANCHOR_HEAD, 0),
                                     new MetaEnumEntryInfo_s(MetaName.ANCHOR_EYES, 1),
                                     new MetaEnumEntryInfo_s(MetaName.ANCHOR_EARS, 2),
                                     new MetaEnumEntryInfo_s(MetaName.ANCHOR_MOUTH, 3),
                                     new MetaEnumEntryInfo_s(MetaName.ANCHOR_LEFT_HAND, 4),
                                     new MetaEnumEntryInfo_s(MetaName.ANCHOR_RIGHT_HAND, 5),
                                     new MetaEnumEntryInfo_s(MetaName.ANCHOR_LEFT_WRIST, 6),
                                     new MetaEnumEntryInfo_s(MetaName.ANCHOR_RIGHT_WRIST, 7),
                                     new MetaEnumEntryInfo_s(MetaName.ANCHOR_HIP, 8),
                                     new MetaEnumEntryInfo_s(MetaName.ANCHOR_LEFT_FOOT, 9),
                                     new MetaEnumEntryInfo_s(MetaName.ANCHOR_RIGHT_FOOT, 10),
                                     new MetaEnumEntryInfo_s(MetaName.ANCHOR_PH_L_HAND, 11),
                                     new MetaEnumEntryInfo_s(MetaName.ANCHOR_PH_R_HAND, 12),
                                     new MetaEnumEntryInfo_s(MetaName.NUM_ANCHORS, 13)
                                    ),
                MetaName.rage__phCapsuleBoundDef__enCollisionBoundDef => new MetaEnumInfo(MetaName.rage__phCapsuleBoundDef__enCollisionBoundDef, 1585854303,
                                     new MetaEnumEntryInfo_s(MetaName.BOUND_DEF_IS_PLANE, 0)
                                    ),
                _ => null,
            };
        }




        private static string GetSafeName(MetaName namehash, uint key)
        {
            string name = namehash.ToString();
            if (string.IsNullOrEmpty(name))
            {
                name = $"Unk_{key}";
            }
            if (!char.IsLetter(name[0]))
            {
                name = $"Unk_{name}";
            }
            return name;
        }





        public static byte[] ConvertToBytes<T>(in T item) where T : struct
        {
            int size = Marshal.SizeOf(typeof(T));
            //int offset = 0;
            byte[] arr = new byte[size];
            MemoryMarshal.TryWrite(arr.AsSpan(), in item);
            return arr;
            //IntPtr ptr = Marshal.AllocHGlobal(size);
            //Marshal.StructureToPtr(item, ptr, true);
            //Marshal.Copy(ptr, arr, 0, size);
            //Marshal.FreeHGlobal(ptr);
            //offset += size;
            //return arr;
        }

        [return: NotNullIfNotNull(nameof(items))]
        public static byte[]? ConvertArrayToBytes<T>(params T[]? items) where T : struct
        {
            if (items is null)
                return null;

            return MemoryMarshal.AsBytes(items.AsSpan()).ToArray();
        }

        public static Span<byte> ConvertArrayToBytes<T>(Span<T> items) where T : struct
        {
            return MemoryMarshal.AsBytes(items);
        }


        //public static T ConvertData<T>(byte[] data) where T : struct
        //{
        //    MemoryMarshal.TryRead<T>(data.AsSpan(), out T value);

        //    return value;
        //    //GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
        //    //var h = handle.AddrOfPinnedObject();
        //    //var r = Marshal.PtrToStructure<T>(h);
        //    //handle.Free();
        //    //return r;
        //}

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T ConvertData<T>(Span<byte> data) where T : struct
        {
            TryConvertData(data, out T value);

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T ConvertData<T>(byte[] data, int offset) where T : struct
        {
            TryConvertData(data.AsSpan(offset), out T value);

            return value;
            //GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
            //var h = handle.AddrOfPinnedObject();
            //var r = Marshal.PtrToStructure<T>(h + offset);
            //handle.Free();
            //return r;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryConvertData<T>(Span<byte> data, out T value) where T : struct
        {
            return MemoryMarshal.TryRead<T>(data, out value);
        }

        public static bool TryConvertData<T>(byte[] data, int offset, out T value) where T : struct
        {
            return TryConvertData<T>(data.AsSpan(offset), out value);
        }

        public static Span<T> ConvertDataArray<T>(Span<byte> data, int count) where T : struct
        {
            return MemoryMarshal.Cast<byte, T>(data.Slice(0, count * Marshal.SizeOf(typeof(T))));
        }

        public static Span<T> ConvertDataArray<T>(byte[] data, int offset, int count) where T : struct
        {
            //T[] items = new T[count];
            //int itemsize = Marshal.SizeOf(typeof(T));
            ////for (int i = 0; i < count; i++)
            ////{
            ////    int off = offset + i * itemsize;
            ////    items[i] = ConvertData<T>(data, off);
            ////}
            //GCHandle handle = GCHandle.Alloc(items, GCHandleType.Pinned);
            //var h = handle.AddrOfPinnedObject();
            //Marshal.Copy(data, offset, h, itemsize * count);
            //handle.Free();

            //return items;
            return MemoryMarshal.Cast<byte, T>(data.AsSpan(offset, count * Marshal.SizeOf(typeof(T))));
            //T[] items = new T[count];
            //int itemsize = Marshal.SizeOf(typeof(T));
            //for (int i = 0; i < count; i++)
            //{
            //    int off = offset + i * itemsize;
            //    items[i] = ConvertData<T>(data, off);
            //}
            //GCHandle handle = GCHandle.Alloc(items, GCHandleType.Pinned);
            //var h = handle.AddrOfPinnedObject();
            //Marshal.Copy(data, offset, h, itemsize * count);
            //handle.Free();

            //return items;
        }

        public static T[]? ConvertDataArray<T>(Meta meta, MetaName name, in Array_StructurePointer array, T[]? buffer = null) where T : struct
        {
            //return ConvertDataArray<T>(meta, name, array.Pointer, array.Count1);
            var count = (int)array.Count1;
            if (count == 0)
                return null;

            var ptrsArr = ArrayPool<MetaPOINTER>.Shared.Rent(count);
            try
            {
                Span<MetaPOINTER> ptrs = GetPointerArray(meta, in array, ptrsArr).AsSpan(0, count);
                if (ptrs.IsEmpty)
                    return null;
                if (ptrs.Length < count)
                {
                    return null;
                }

                T[] items = buffer ?? new T[count];

                //MetaName blocktype = 0;
                for (int i = 0; i < count; i++)
                {
                    var ptr = ptrs[i];
                    var offset = ptr.Offset;
                    var block = meta.GetBlock(ptr.BlockID);
                    if (block is null)
                    {
                        continue;
                    }

                    if (block.StructureNameHash != name)
                    {
                        return null;
                    } //type mismatch - don't return anything...
                    if (offset < 0 || block.Data is null || offset >= block.Data.Length)
                    {
                        continue;
                    }
                    TryConvertData<T>(block.Data.AsSpan(offset), out items[i]);
                }

                return items;
            }
            finally
            {
                ArrayPool<MetaPOINTER>.Shared.Return(ptrsArr);
            }
        }

        public delegate void DataArrayAction<T, TArg>(Span<T> span, ref TArg arg);

        public static void ConvertDataArrayAction<T, TState>(Meta meta, MetaName name, in Array_Structure array, ref TState state, DataArrayAction<T, TState> action) where T : struct
        {
            ConvertDataArrayAction(meta, name, array.Pointer, array.Count1, ref state, action);
        }

        public static T[]? ConvertDataArray<T>(Meta meta, MetaName name, in Array_Structure array, T[]? buffer = null) where T : struct
        {
            return ConvertDataArray<T>(meta, name, array.Pointer, array.Count1, buffer);
        }

        public static void ConvertDataArrayAction<T, TState>(Meta meta, MetaName name, ulong pointer, uint count, ref TState state, DataArrayAction<T, TState> action) where T : struct
        {
            if (count == 0)
                return;

            int itemsize = Marshal.SizeOf(typeof(T));
            int itemsleft = (int)count; //large arrays get split into chunks...

            uint ptrindex = (uint)(pointer & 0xFFF) - 1;
            uint ptroffset = (uint)((pointer >> 12) & 0xFFFFF);
            var ptrblock = (ptrindex < meta.DataBlocks.Count) ? meta.DataBlocks[(int)ptrindex] : null;
            if (ptrblock?.Data is null || ptrblock.StructureNameHash != name)
            {
                return;
            } //no block or wrong block? shouldn't happen!

            int byteoffset = (int)ptroffset;// (ptroffset * 16 + ptrunkval);
            int itemoffset = byteoffset / itemsize;

            int curi = 0;
            while (itemsleft > 0)
            {
                int blockcount = ptrblock.DataLength / itemsize;
                int itemcount = blockcount - itemoffset;
                if (itemcount > itemsleft)
                {
                    itemcount = itemsleft;
                } //don't try to read too many items..

                action(ConvertDataArray<T>(ptrblock.Data, itemoffset * Marshal.SizeOf(typeof(T)), itemcount), ref state);
                //for (int i = 0; i < itemcount; i++)
                //{
                //    int offset = (itemoffset + i) * itemsize;
                //    int index = curi + i;
                //    items[index] = ConvertData<T>(ptrblock.Data, offset);
                //}
                itemoffset = 0; //start at beginning of next block..
                curi += itemcount;
                itemsleft -= itemcount;
                if (itemsleft <= 0)
                {
                    return;
                }//all done!
                ptrindex++;
                ptrblock = (ptrindex < meta.DataBlocks.Count) ? meta.DataBlocks[(int)ptrindex] : null;
                if (ptrblock?.Data is null || ptrblock.StructureNameHash != name)
                {
                    break;
                } //not enough items..?
            }

            return;
        }

        public static T[]? ConvertDataArray<T>(Meta meta, MetaName name, ulong pointer, uint count, T[]? buffer = null) where T : struct
        {
            if (count == 0)
                return null;

            T[] items = buffer ?? GC.AllocateUninitializedArray<T>((int)count);
            int itemsize = Marshal.SizeOf(typeof(T));
            int itemsleft = (int)count; //large arrays get split into chunks...

            uint ptrindex = (uint)(pointer & 0xFFF) - 1;
            uint ptroffset = (uint)((pointer >> 12) & 0xFFFFF);
            var ptrblock = (ptrindex < meta.DataBlocks.Count) ? meta.DataBlocks[(int)ptrindex] : null;
            if (ptrblock?.Data is null || ptrblock.StructureNameHash != name)
            {
                return null;
            } //no block or wrong block? shouldn't happen!

            int byteoffset = (int)ptroffset;// (ptroffset * 16 + ptrunkval);
            int itemoffset = byteoffset / itemsize;

            int curi = 0;
            while (itemsleft > 0)
            {
                int blockcount = ptrblock.DataLength / itemsize;
                int itemcount = blockcount - itemoffset;
                if (itemcount > itemsleft)
                {
                    itemcount = itemsleft;
                } //don't try to read too many items..

                ConvertDataArray<T>(ptrblock.Data, itemoffset * Marshal.SizeOf(typeof(T)), itemcount).CopyTo(items.AsSpan(curi));
                //for (int i = 0; i < itemcount; i++)
                //{
                //    int offset = (itemoffset + i) * itemsize;
                //    int index = curi + i;
                //    items[index] = ConvertData<T>(ptrblock.Data, offset);
                //}
                itemoffset = 0; //start at beginning of next block..
                curi += itemcount;
                itemsleft -= itemcount;
                if (itemsleft <= 0)
                {
                    return items;
                }//all done!
                ptrindex++;
                ptrblock = (ptrindex < meta.DataBlocks.Count) ? meta.DataBlocks[(int)ptrindex] : null;
                if (ptrblock?.Data is null || ptrblock.StructureNameHash != name)
                {
                    break;
                } //not enough items..?
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

        public static MetaPOINTER[] GetPointerArray(this Meta meta, in Array_StructurePointer array, MetaPOINTER[]? buffer = null)
        {
            uint count = array.Count1;
            if (count == 0)
                return [];

            int ptrsize = Marshal.SizeOf(typeof(MetaPOINTER));
            int ptroffset = (int)array.PointerDataOffset;
            var ptrblock = meta.GetBlock((int)array.PointerDataId);
            if (ptrblock?.Data == null || ptrblock.StructureNameHash != (MetaName)MetaTypeName.POINTER)
            {
                return [];
            }

            MetaPOINTER[] ptrs = buffer ?? GC.AllocateUninitializedArray<MetaPOINTER>((int)count);
            for (int i = 0; i < count; i++)
            {
                int offset = ptroffset + (i * ptrsize);
                if (offset >= ptrblock.Data.Length)
                {
                    break;
                }
                TryConvertData(ptrblock.Data.AsSpan(offset), out ptrs[i]);
            }

            return ptrs;
        }

        //public static MetaPOINTER[]? GetPointerArray(this Meta meta, Array_StructurePointer array, MetaPOINTER[]? buffer = null)
        //{
        //    return GetPointerArray(meta, in array, buffer);
        //}

        public static MetaHash[]? GetHashArray(this Meta meta, in Array_uint array)
        {
            return ConvertDataArray<MetaHash>(meta, (MetaName)MetaTypeName.HASH, array.Pointer, array.Count1);
        }
        public static Vector4[]? GetPaddedVector3Array(Meta meta, in Array_Vector3 array)
        {
            return ConvertDataArray<Vector4>(meta, (MetaName)MetaTypeName.VECTOR4, array.Pointer, array.Count1);
        }
        public static uint[]? GetUintArray(Meta meta, in Array_uint array)
        {
            return ConvertDataArray<uint>(meta, (MetaName)MetaTypeName.UINT, array.Pointer, array.Count1);
        }
        public static ushort[]? GetUshortArray(Meta meta, in Array_ushort array)
        {
            return ConvertDataArray<ushort>(meta, (MetaName)MetaTypeName.USHORT, array.Pointer, array.Count1);
        }
        public static float[]? GetFloatArray(this Meta meta, in Array_float array)
        {
            return ConvertDataArray<float>(meta, (MetaName)MetaTypeName.FLOAT, array.Pointer, array.Count1);
        }

        public static byte[]? GetByteArray(this Meta meta, in Array_byte array)
        {
            uint ptrindex = array.PointerDataIndex;
            uint ptroffset = array.PointerDataOffset;

            if (meta.DataBlocks is null || ptrindex >= meta.DataBlocks.Count)
                return null;

            var ptrblock = meta.DataBlocks[(int)ptrindex];
            if (ptrblock?.Data is null)// || (ptrblock.StructureNameHash != name))
                return null; //no block or wrong block? shouldn't happen!

            var count = array.Count1;
            if (ptroffset + count > ptrblock.Data.Length)
                return null;

            byte[] data = GC.AllocateUninitializedArray<byte>(count);
            Buffer.BlockCopy(ptrblock.Data, (int)ptroffset, data, 0, count);
            return data;
        }

        public static byte[]? GetByteArray(Meta meta, in DataBlockPointer ptr, uint count)
        {
            //var pointer = array.Pointer;
            uint ptrindex = ptr.PointerDataIndex;// (pointer & 0xFFF) - 1;
            uint ptroffset = ptr.PointerDataOffset;// ((pointer >> 12) & 0xFFFFF);

            if (meta.DataBlocks is null || ptrindex >= meta.DataBlocks.Count)
                return null;

            var ptrblock = meta.DataBlocks[(int)ptrindex];
            if (ptrblock?.Data is null) // || (ptrblock.StructureNameHash != name))
                return null; //no block or wrong block? shouldn't happen!

            if (ptroffset + count > ptrblock.Data.Length)
                return null;

            byte[] data = GC.AllocateUninitializedArray<byte>((int)count);
            Buffer.BlockCopy(ptrblock.Data, (int)ptroffset, data, 0, (int)count);
            return data;
        }


        public static T[]? GetTypedDataArray<T>(Meta meta, MetaName name) where T : struct
        {
            if (meta?.DataBlocks is null)
                return null;

            var datablocks = meta.DataBlocks.Data;

            MetaDataBlock? startblock = null;
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
            if (startblock is null)
            {
                return null; //couldn't find the data.
            }

            int count = 0; //try figure out how many items there are, from the block size(s).
            int itemsize = Marshal.SizeOf(typeof(T));
            var currentblock = startblock;
            int currentblockind = startblockind;
            while (currentblock is not null)
            {
                int blockitems = currentblock.DataLength / itemsize;
                count += blockitems;
                currentblockind++;
                if (currentblockind >= datablocks.Count)
                    break; //last block, can't go any further

                currentblock = datablocks[currentblockind];
                if (currentblock.StructureNameHash != name)
                    break; //not the right block type, can't go further
            }

            if (count <= 0)
            {
                return null; //didn't find anything...
            }

            return ConvertDataArray<T>(meta, name, (uint)startblockind + 1, (uint)count);
        }

        [DoesNotReturn]
        private static void ThrowBlockNotFoundException(MetaName name)
        {
            throw new Exception($"Couldn't find {name} block.");
        }

        public static T GetTypedData<T>(Meta meta, MetaName name) where T : struct
        {
            ArgumentNullException.ThrowIfNull(meta, nameof(meta));
            if (meta?.DataBlocks is null)
            {
                ThrowHelper.ThrowInvalidOperationException($"meta.DataBlocks is null!");
            }
            
            foreach (var block in meta.DataBlocks.Span)
            {
                if (block.StructureNameHash == name)
                {
                    return ConvertData<T>(block.Data);
                }
            }
            //foreach (var block in meta.DataBlocks)
            //{
            //    if (block.StructureNameHash == name)
            //    {
            //        return MetaTypes.ConvertData<T>(block.Data);
            //    }
            //}

            ThrowBlockNotFoundException(name);
            return default;
        }
        public static string[]? GetStrings(this Meta? meta)
        {
            //look for strings in the sectionSTRINGS data block(s)

            if (meta?.DataBlocks is null)
                return null;

            var datablocks = meta.DataBlocks.Data;

            MetaDataBlock? startblock = null;
            int startblockind = -1;
            for (int i = 0; i < datablocks.Count; i++)
            {
                var block = datablocks[i];
                if (block.StructureNameHash == (MetaName)MetaTypeName.STRING)
                {
                    startblock = block;
                    startblockind = i;
                    break;
                }
            }
            if (startblock is null)
            {
                return null; //couldn't find the strings data section.
            }

            PooledList<string> strings = PooledListPool<string>.Shared.Get();
            try
            {
                var currentblock = startblock;
                int currentblockind = startblockind;
                while (currentblock != null)
                {
                    //read strings from the block.
                    int startindex = 0;
                    int endindex = 0;
                    var data = currentblock.Data;
                    foreach (var span in data.AsSpan().EnumerateSplit((byte)0))
                    {
                        if (!span.IsEmpty)
                        {
                            string str = Encoding.ASCII.GetStringPooled(span);
                            strings.Add(str);
                        }
                    }
                    //for (int b = 0; b < data.Length; b++)
                    //{
                    //    if (data[b] == 0)
                    //    {
                    //        startindex = endindex;
                    //        endindex = b;
                    //        if (endindex > startindex)
                    //        {
                    //            string str = Encoding.ASCII.GetString(data.AsSpan(startindex, endindex - startindex));
                    //            strings.Add(str);
                    //            endindex++; //start next string after the 0.
                    //        }
                    //    }
                    //}
                    //if (endindex != data.Length - 1)
                    //{
                    //    startindex = endindex;
                    //    endindex = data.Length - 1;
                    //    if (endindex > startindex)
                    //    {
                    //        string str = Encoding.ASCII.GetString(data.AsSpan(startindex, endindex - startindex));
                    //        strings.Add(str);
                    //        strings2.Add(str);
                    //    }
                    //}

                    currentblockind++;
                    if (currentblockind >= datablocks.Count)
                        break; //last block, can't go any further

                    currentblock = datablocks[currentblockind];
                    if (currentblock.StructureNameHash != (MetaName)MetaTypeName.STRING)
                        break; //not the right block type, can't go further
                }


                if (strings.Count <= 0)
                {
                    return null; //don't return empty array...
                }
                return strings.ToArray();
            }
            finally
            {
                PooledListPool<string>.Shared.Return(strings);
            }
        }

        [SkipLocalsInit]
        public static string? GetString(this Meta meta, in CharPointer ptr)
        {
            var blocki = (int)ptr.PointerDataIndex;// (ptr.Pointer & 0xFFF) - 1;
            var offset = (int)ptr.PointerDataOffset;// (ptr.Pointer >> 12) & 0xFFFFF;
            if ((blocki < 0) || (blocki >= meta.DataBlocks.BlockLength))
            {
                return null;
            }
            var block = meta.DataBlocks[blocki];
            if (block.StructureNameHash != (MetaName)MetaTypeName.STRING)
            {
                return null;
            }
            //var byteoffset = offset * 16 + offset2;
            var length = ptr.Count1;
            var lastbyte = offset + length;
            if (lastbyte >= block.DataLength)
            {
                return null;
            }

            //string s = Encoding.ASCII.GetString(block.Data.AsSpan(offset, length));

            //if (meta.Strings == null) return null;
            //if (offset < 0) return null;
            //if (offset >= meta.Strings.Length) return null;
            //string s = meta.Strings[offset];

            return Encoding.ASCII.GetStringPooled(block.Data.AsSpan(offset, length));
        }

        public static string? GetString(this Meta meta, CharPointer ptr)
        {
            return GetString(meta, in ptr);
        }

        public static MetaWrapper[]? GetExtensions(Meta meta, in Array_StructurePointer ptr)
        {
            if (ptr.Count1 == 0)
                return null;
            var result = new MetaWrapper[ptr.Count1];
            var ptrs = ArrayPool<MetaPOINTER>.Shared.Rent(ptr.Count1);
            try
            {
                GetPointerArray(meta, in ptr, ptrs);
                var extptrs = ptrs.AsSpan(0, ptr.Count1);
                if (!extptrs.IsEmpty)
                {
                    for (int i = 0; i < extptrs.Length; i++)
                    {
                        ref var extptr = ref extptrs[i];
                        var block = meta.GetBlock(extptr.BlockID);
                        var h = block?.StructureNameHash ?? 0;

                        //var extptr = extptrs[i];
                        MetaWrapper? ext = h switch
                        {
                            //archetype extension types
                            MetaName.CExtensionDefParticleEffect => new MCExtensionDefParticleEffect(),// MetaExtension<CExtensionDefParticleEffect>(h, GetData<CExtensionDefParticleEffect>(block, extptr));
                            MetaName.CExtensionDefAudioCollisionSettings => new MCExtensionDefAudioCollisionSettings(),// MetaExtension<CExtensionDefAudioCollisionSettings>(h, GetData<CExtensionDefAudioCollisionSettings>(block, extptr));
                            MetaName.CExtensionDefAudioEmitter => new MCExtensionDefAudioEmitter(),// MetaExtension<CExtensionDefAudioEmitter>(h, GetData<CExtensionDefAudioEmitter>(block, extptr));
                            MetaName.CExtensionDefSpawnPoint => new MCExtensionDefSpawnPoint(),// new MetaExtension<CExtensionDefSpawnPoint>(h, GetData<CExtensionDefSpawnPoint>(block, extptr));
                            MetaName.CExtensionDefExplosionEffect => new MCExtensionDefExplosionEffect(),// MetaExtension<CExtensionDefExplosionEffect>(h, GetData<CExtensionDefExplosionEffect>(block, extptr));
                            MetaName.CExtensionDefLadder => new MCExtensionDefLadder(),// MetaExtension<CExtensionDefLadder>(h, GetData<CExtensionDefLadder>(block, extptr));
                            MetaName.CExtensionDefBuoyancy => new MCExtensionDefBuoyancy(),// MetaExtension<CExtensionDefBuoyancy>(h, GetData<CExtensionDefBuoyancy>(block, extptr));
                            MetaName.CExtensionDefExpression => new MCExtensionDefExpression(),// MetaExtension<CExtensionDefExpression>(h, GetData<CExtensionDefExpression>(block, extptr));
                            MetaName.CExtensionDefLightShaft => new MCExtensionDefLightShaft(),// MetaExtension<CExtensionDefLightShaft>(h, GetData<CExtensionDefLightShaft>(block, extptr));
                            MetaName.CExtensionDefWindDisturbance => new MCExtensionDefWindDisturbance(),// MetaExtension<CExtensionDefWindDisturbance>(h, GetData<CExtensionDefWindDisturbance>(block, extptr));
                            MetaName.CExtensionDefProcObject => new MCExtensionDefProcObject(),// MetaExtension<CExtensionDefProcObject>(h, GetData<CExtensionDefProcObject>(block, extptr));
                                                                                               //entity extension types
                            MetaName.CExtensionDefLightEffect => new MCExtensionDefLightEffect(),// MetaExtension<CExtensionDefLightEffect>(h, GetData<CExtensionDefLightEffect>(block, extptr));
                            MetaName.CExtensionDefSpawnPointOverride => new MCExtensionDefSpawnPointOverride(),// MetaExtension<CExtensionDefSpawnPointOverride>(h, GetData<CExtensionDefSpawnPointOverride>(block, extptr));
                            MetaName.CExtensionDefDoor => new MCExtensionDefDoor(),// MetaExtension<CExtensionDefDoor>(h, GetData<CExtensionDefDoor>(block, extptr));
                                                                                   //rage__phVerletClothCustomBounds
                            MetaName.rage__phVerletClothCustomBounds => new Mrage__phVerletClothCustomBounds(),// MetaExtension<rage__phVerletClothCustomBounds>(h, GetData<rage__phVerletClothCustomBounds>(block, extptr));
                            _ => null,
                        };

                        //string ts = GetTypesInitString(meta);

                        ext?.Load(meta, in extptr);
                        if (i < result.Length)
                        {
                            result[i] = ext;
                        }
                    }
                }
            }
            finally
            {
                ArrayPool<MetaPOINTER>.Shared.Return(ptrs);
            }
            return result;
        }


        public static int GetDataOffset(MetaDataBlock? block, in MetaPOINTER ptr)
        {
            if (block is null || block.Data is null)
                return -1;

            var offset = ptr.Offset;
            if (offset < 0 || offset >= block.Data.Length)
                return -1;

            return offset;
        }

        public static T GetData<T>(Meta meta, in MetaPOINTER ptr) where T : struct
        {
            _ = TryGetData<T>(meta, in ptr, out var result);

            return result;
        }

        public static bool TryGetData<T>(Meta meta, in MetaPOINTER ptr, out T result) where T : struct
        {
            var block = meta.GetBlock(ptr.BlockID); 
            var offset = GetDataOffset(block, in ptr);
            if (offset < 0 || block is null)
            {
                result = default;
                return false;
            }

            return TryConvertData(block.Data, out result);
        }

        public static ushort SwapBytes(ushort x)
        {
            return BinaryPrimitives.ReverseEndianness(x);
        }
        public static short SwapBytes(short x)
        {
            return BinaryPrimitives.ReverseEndianness(x);
        }
        public static uint SwapBytes(uint x)
        {
            return BinaryPrimitives.ReverseEndianness(x);
        }
        public static int SwapBytes(int x)
        {
            return BinaryPrimitives.ReverseEndianness(x);
        }
        public static ulong SwapBytes(ulong x)
        {
            ////////// [not swapping 32bit blocks! careful!]
            //////// swap adjacent 32-bit blocks
            //////x = (x >> 32) | (x << 32);
            //// swap adjacent 16-bit blocks
            x = ((x & 0xFFFF0000FFFF0000) >> 16) | ((x & 0x0000FFFF0000FFFF) << 16);
            //// swap adjacent 8-bit blocks
            return ((x & 0xFF00FF00FF00FF00) >> 8) | ((x & 0x00FF00FF00FF00FF) << 8);

            //return (ulong)((ulong)(BinaryPrimitives.ReverseEndianness((uint)(x >> 32)) << 32) | (ulong)BinaryPrimitives.ReverseEndianness((uint)(x & 0xFFFFFFFF)));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float SwapBytes(float f)
        {
            return BitConverter.Int32BitsToSingle(BinaryPrimitives.ReverseEndianness(BitConverter.SingleToInt32Bits(f)));
        }
        public static Vector2 SwapBytes(in Vector2 v)
        {
            return new Vector2(SwapBytes(v.X), SwapBytes(v.Y));
        }
        public static Vector3 SwapBytes(in Vector3 v)
        {
            return new Vector3(SwapBytes(v.X), SwapBytes(v.Y), SwapBytes(v.Z));
        }
        public static Vector4 SwapBytes(in Vector4 v)
        {
            return new Vector4(SwapBytes(v.X), SwapBytes(v.Y), SwapBytes(v.Z), SwapBytes(v.W));
        }
    }





    public enum MetaTypeName : uint
    {

        VECTOR4 = 0x33,
        HASH = 0x4a,
        STRING = 0x10,
        POINTER = 0x7,
        USHORT = 0x13,
        UINT = 0x15,
        ARRAYINFO = 0x100,
        BYTE = 17,
        FLOAT = 33, //0x21

        PsoPOINTER = 12,

    }




    [TC(typeof(EXP))]
    public abstract class MetaWrapper
    {
        public virtual string Name => ToString();
        public abstract void Load(Meta meta, in MetaPOINTER ptr);
        public abstract MetaPOINTER Save(MetaBuilder mb);
    }




    //generated enums
    
    [Flags] public enum CScenarioPointFlags__Flags //SCENARIO point flags  / extension spawn point flags
        : int //Key:2814596095
    {
        IgnoreMaxInRange = 1,//0,
        NoSpawn = 2,//1,
        StationaryReactions = 4,//2,
        OnlySpawnInSameInterior = 8,//3,
        SpawnedPedIsArrestable = 16,//4,
        ActivateVehicleSiren = 32,//5,
        AggressiveVehicleDriving = 64,//6,
        LandVehicleOnArrival = 128,//7,
        IgnoreThreatsIfLosNotClear = 256,//8,
        EventsInRadiusTriggerDisputes = 512,//9,
        AerialVehiclePoint = 1024,//10,
        TerritorialScenario = 2048,//11,
        EndScenarioIfPlayerWithinRadius = 4096,//12,
        EventsInRadiusTriggerThreatResponse = 8192,//13,
        TaxiPlaneOnGround = 16384,//14,
        FlyOffToOblivion = 32768,//15,
        InWater = 65536,//16,
        AllowInvestigation = 131072,//17,
        OpenDoor = 262144,//18,
        PreciseUseTime = 524288,//19,
        NoRespawnUntilStreamedOut = 1048576,//20,
        NoVehicleSpawnMaxDistance = 2097152,//21,
        ExtendedRange = 4194304,//22,
        ShortRange = 8388608,//23,
        HighPriority = 16777216,//24,
        IgnoreLoitering = 33554432,//25,
        UseSearchlight = 67108864,//26,
        ResetNoCollisionOnCleanUp = 134217728,//27,
        CheckCrossedArrivalPlane = 268435456,//28,
        UseVehicleFrontForArrival = 536870912,//29,
        IgnoreWeatherRestrictions = 1073741824,//30,
    }

    public enum CSpawnPoint__AvailabilityMpSp //SCENARIO Spawn point availability availableInMpSp
        : int //Key:671739257
    {
        kBoth = 0,
        kOnlySp = 1,
        kOnlyMp = 2,
    }

    public enum CScenarioChainingEdge__eAction //SCENARIO (Path) Edge Action
        : byte //Key:3326075799
    {
        Move = 0,
        Unk_7865678 = 1,
        MoveFollowMaster = 2,
    }

    public enum CScenarioChainingEdge__eNavMode //SCENARIO (Path) Edge nav mode
        : byte //Key:3016128742
    {
        Direct = 0,
        NavMesh = 1,
        Roads = 2,
    }

    public enum CScenarioChainingEdge__eNavSpeed //SCENARIO (Path) Edge nav speed
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

    public enum rage__fwArchetypeDef__eAssetType //archetype assetType
        : int //Key:1866031916
    {
        ASSET_TYPE_UNINITIALIZED = 0, //189734893
        ASSET_TYPE_FRAGMENT = 1, //571047911
        ASSET_TYPE_DRAWABLE = 2, //130075505
        ASSET_TYPE_DRAWABLEDICTIONARY = 3, //1580165652
        ASSET_TYPE_ASSETLESS = 4, //4161085041
    }

    public enum rage__eLodType //entity lodLevel
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

    public enum rage__ePriorityLevel //entity priorityLevel
        : int //Key:2200357711
    {
        PRI_REQUIRED = 0,  //1943361227
        PRI_OPTIONAL_HIGH = 1, //3993616791
        PRI_OPTIONAL_MEDIUM = 2, //515598709
        PRI_OPTIONAL_LOW = 3, //329627604
    }

    public enum CExtensionDefLadderMaterialType //archetype CExtensionDefLadder materialType
        : int //Key:3514570158
    {
        METAL_SOLID_LADDER = 0,
        METAL_LIGHT_LADDER = 1,
        WOODEN_LADDER = 2,
    }

    public enum CExtensionDefLightShaftDensityType //archetype CExtensionDefLightShaft densityType
        : int //Key:3539601182
    {
        LIGHTSHAFT_DENSITYTYPE_CONSTANT = 0,
        LIGHTSHAFT_DENSITYTYPE_SOFT = 1,
        LIGHTSHAFT_DENSITYTYPE_SOFT_SHADOW = 2,
        LIGHTSHAFT_DENSITYTYPE_SOFT_SHADOW_HD = 3,
        LIGHTSHAFT_DENSITYTYPE_LINEAR = 4,
        LIGHTSHAFT_DENSITYTYPE_LINEAR_GRADIENT = 5,
        LIGHTSHAFT_DENSITYTYPE_QUADRATIC = 6,
        LIGHTSHAFT_DENSITYTYPE_QUADRATIC_GRADIENT = 7,
    }

    public enum CExtensionDefLightShaftVolumeType //archetype CExtensionDefLightShaft volumeType
        : int //Key:4287472345
    {
        LIGHTSHAFT_VOLUMETYPE_SHAFT = 0,
        LIGHTSHAFT_VOLUMETYPE_CYLINDER = 1,
    }

    public enum ePedVarComp //component peds CComponentInfo ped accessory / variations slot
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

    public enum ePropRenderFlags //component peds CPedPropMetaData renderFlags
        : int //Key:1551913633
    {
        PRF_ALPHA = 0,
        PRF_DECAL = 1,
        PRF_CUTOUT = 2,
    }

    public enum eAnchorPoints //component peds CAnchorProps eAnchorPoints
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
        ANCHOR_PH_L_HAND = 11,
        ANCHOR_PH_R_HAND = 12,
        NUM_ANCHORS = 13,
    }

    public enum rage__phCapsuleBoundDef__enCollisionBoundDef //cloth collision data - rage__phCapsuleBoundDef Flags
        : int //Key:1585854303
    {
        BOUND_DEF_IS_PLANE = 0,
    }






    //generated + adjusted structs code (UnusedX padding vars manually added) from here down, + meta wrapper classes

    [TC(typeof(EXP))]
    public struct CMapTypes //80 bytes, Key:2608875220
    {
        public uint Unused0;//0
        public uint Unused1;//4
        public Array_StructurePointer extensions; //8   8: Array: 0: extensions  {0: StructurePointer: 0: 256}
        public Array_StructurePointer archetypes; //24   24: Array: 0: archetypes  {0: StructurePointer: 0: 256}
        public MetaHash name; //40   40: Hash: 0: name
        public uint Unused2;//44
        public Array_uint dependencies; //48   48: Array: 0: dependencies//1013942340  {0: Hash: 0: 256}
        public Array_Structure compositeEntityTypes; //64   64: Array: 0: compositeEntityTypes  {0: Structure: SectionUNKNOWN2: 256}

        public override readonly string ToString() => name.ToString();
    }

    [TC(typeof(EXP))]
    public struct CBaseArchetypeDef //144 bytes, Key:2411387556
    {
        public uint Unused00 { get; set; }//0
        public uint Unused01 { get; set; }//4
        public float lodDist { get; set; } //8   8: Float: 0: lodDist
        public uint flags { get; set; } //12   12: UnsignedInt: 0: flags
        public uint specialAttribute { get; set; } //16   16: UnsignedInt: 0: specialAttribute
        public uint Unused02 { get; set; }//20
        public uint Unused03 { get; set; }//24
        public uint Unused04 { get; set; }//28
        public Vector3 bbMin; //32   32: Float_XYZ: 0: bbMin
        public float Unused05 { get; set; }//44
        public Vector3 bbMax; //48   48: Float_XYZ: 0: bbMax
        public float Unused06 { get; set; }//60
        public Vector3 bsCentre; //64   64: Float_XYZ: 0: bsCentre
        public float Unused07 { get; set; }//76
        public float bsRadius { get; set; } //80   80: Float: 0: bsRadius
        public float hdTextureDist { get; set; } //84   84: Float: 0: hdTextureDist//2908576588
        public MetaHash name { get; set; } //88   88: Hash: 0: name
        public MetaHash textureDictionary { get; set; } //92   92: Hash: 0: textureDictionary
        public MetaHash clipDictionary { get; set; } //96   96: Hash: 0: clipDictionary//424089489
        public MetaHash drawableDictionary { get; set; } //100   100: Hash: 0: drawableDictionary
        public MetaHash physicsDictionary { get; set; } //104   104: Hash: 0: physicsDictionary//3553040380
        public rage__fwArchetypeDef__eAssetType assetType { get; set; } //108   108: IntEnum: 1991964615: assetType
        public MetaHash assetName { get; set; } //112   112: Hash: 0: assetName
        public uint Unused08 { get; set; }//116
        public Array_StructurePointer extensions; //120   120: Array: 0: extensions  {0: StructurePointer: 0: 256}
        public uint Unused09 { get; set; }//136
        public uint Unused10 { get; set; }//140


        public override readonly string ToString() => $"{name}, {assetName}, {drawableDictionary}, {textureDictionary}";
    }

    [TC(typeof(EXP))]
    public struct CBaseArchetypeDef_v2 //128 bytes, Key:2352343492  //old version...
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

    [TC(typeof(EXP))]
    public struct CTimeArchetypeDefData
    {
        public uint timeFlags { get; set; } //144   144: UnsignedInt: 0: timeFlags//2248791340
        public uint Unused11 { get; set; }//148
        public uint Unused12 { get; set; }//152
        public uint Unused13 { get; set; }//156
    }

    [TC(typeof(EXP))]
    public struct CTimeArchetypeDef //160 bytes, Key:2520619910
    {
        public CBaseArchetypeDef _BaseArchetypeDef;
        public CTimeArchetypeDefData _TimeArchetypeDef;
        public readonly CBaseArchetypeDef BaseArchetypeDef => _BaseArchetypeDef;
        public readonly CTimeArchetypeDefData TimeArchetypeDef => _TimeArchetypeDef;

        public override readonly string ToString() => _BaseArchetypeDef.ToString();
    }

    [TC(typeof(EXP))]
    public struct CMloArchetypeDefData
    {
        public uint mloFlags { get; set; } //144   144: UnsignedInt: 0: mloFlags//3590839912
        public uint Unused11 { get; set; }//148
        public Array_StructurePointer entities; //152   152: Array: 0: entities  {0: StructurePointer: 0: 256}
        public Array_Structure rooms; //168   168: Array: 0: rooms  {0: Structure: CMloRoomDef: 256}
        public Array_Structure portals; //184   184: Array: 0: portals//2314725778  {0: Structure: CMloPortalDef: 256}
        public Array_Structure entitySets; //200   200: Array: 0: entitySets//1169996080  {0: Structure: CMloEntitySet: 256}
        public Array_Structure timeCycleModifiers; //216   216: Array: 0: timeCycleModifiers  {0: Structure: CMloTimeCycleModifier: 256}
        public uint Unused12 { get; set; }//232
        public uint Unused13 { get; set; }//236
    }

    [TC(typeof(EXP))]
    public struct CMloArchetypeDef //240 bytes, Key:937664754
    {
        public CBaseArchetypeDef _BaseArchetypeDef;
        public CMloArchetypeDefData _MloArchetypeDef;
        public readonly CBaseArchetypeDef BaseArchetypeDef => _BaseArchetypeDef;
        public readonly CMloArchetypeDefData MloArchetypeDef => _MloArchetypeDef;

        public override readonly string ToString() => _BaseArchetypeDef.ToString();
    }

    [TC(typeof(EXP))]
    public struct CMloInstanceDef //160 bytes, Key:2151576752
    {
        public CEntityDef CEntityDef;
        public uint groupId { get; set; } //128   128: UnsignedInt: 0: 2501631252
        public uint floorId { get; set; } //132   132: UnsignedInt: 0: floorId//2187650609
        public Array_uint defaultEntitySets; //136   136: Array: 0: defaultEntitySets//1407157833  {0: Hash: 0: 256}
        public uint numExitPortals { get; set; } //152   152: UnsignedInt: 0: numExitPortals//528711607
        public uint MLOInstflags { get; set; } //156   156: UnsignedInt: 0: MLOInstflags//3761966250
    }

    [TC(typeof(EXP))]
    public struct CMloRoomDef //112 bytes, Key:3885428245
    {
        public uint Unused0 { get; set; }//0
        public uint Unused1 { get; set; }//4
        public CharPointer name; //8   8: CharPointer: 0: name
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
        public int exteriorVisibiltyDepth { get; set; } //88   88: SignedInt: 0: exteriorVisibiltyDepth//552849982
        public uint Unused6 { get; set; }//92
        public Array_uint attachedObjects; //96   96: Array: 0: attachedObjects//2382704940  {0: UnsignedInt: 0: 256}
    }

    [TC(typeof(EXP))]
    public class MCMloRoomDef : MetaWrapper
    {
        public CMloRoomDef _Data;
        public CMloRoomDef Data { get { return _Data; } }
        public string RoomName { get; set; }
        public uint[] AttachedObjects { get; set; }

        public Vector3 BBCenter { get { return (_Data.bbMax + _Data.bbMin) * 0.5f; } }
        public Vector3 BBSize { get { return (_Data.bbMax - _Data.bbMin); } }
        public Vector3 BBMin { get { return (_Data.bbMin); } }
        public Vector3 BBMax { get { return (_Data.bbMax); } }
        public Vector3 BBMin_CW { get; set; }
        public Vector3 BBMax_CW { get; set; }
        
        public MloArchetype OwnerMlo { get; set; } // for browsing/reference purposes
        public int Index { get; set; }

        public MCMloRoomDef() { }
        public MCMloRoomDef(Meta meta, CMloRoomDef data)
        {
            _Data = data;
            RoomName = MetaTypes.GetString(meta, in _Data.name);
            AttachedObjects = MetaTypes.GetUintArray(meta, in _Data.attachedObjects) ?? Array.Empty<uint>();
        }

        public override void Load(Meta meta, in MetaPOINTER ptr)
        {
            MetaTypes.TryGetData(meta, in ptr, out _Data);
            RoomName = MetaTypes.GetString(meta, in _Data.name);
            AttachedObjects = MetaTypes.GetUintArray(meta, in _Data.attachedObjects) ?? Array.Empty<uint>();
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

            _Data.attachedObjects = mb.AddUintArrayPtr(AttachedObjects);

            mb.AddStructureInfo(MetaName.CMloRoomDef);
            return mb.AddItemPtr(MetaName.CMloRoomDef, in _Data);
        }

        public override string Name => RoomName;

        public override string ToString() => RoomName;
    }

    [TC(typeof(EXP))]
    public struct CMloPortalDef //64 bytes, Key:1110221513
    {
        public uint Unused0 { get; set; }//0
        public uint Unused1 { get; set; }//4
        public uint roomFrom { get; set; } //8   8: UnsignedInt: 0: 4101034749
        public uint roomTo { get; set; } //12   12: UnsignedInt: 0: 2607060513
        public uint flags { get; set; } //16   16: UnsignedInt: 0: flags
        public uint mirrorPriority { get; set; } //20   20: UnsignedInt: 0: 1185490713
        public uint opacity { get; set; } //24   24: UnsignedInt: 0: opacity
        public uint audioOcclusion { get; set; } //28   28: UnsignedInt: 0: 1093790004
        public Array_Vector3 corners; //32   32: Array: 0: corners  {0: Float_XYZ: 0: 256}
        public Array_uint attachedObjects; //48   48: Array: 0: attachedObjects//2382704940  {0: UnsignedInt: 0: 256}
    }

    [TC(typeof(EXP))]
    public class MCMloPortalDef : MetaWrapper
    {
        public CMloPortalDef _Data;
        public CMloPortalDef Data => _Data;
        public Vector4[] Corners { get; set; }
        public uint[] AttachedObjects { get; set; }

        public Vector3 Center
        {
            get
            {
                if (Corners == null || Corners.Length == 0)
                    return Vector3.Zero;
                var v = Vector3.Zero;
                for (int i = 0; i < Corners.Length; i++)
                {
                    v += Corners[i].XYZ();
                }
                v *= (1.0f / Corners.Length);
                return v;
            }
        }

        public MloArchetype OwnerMlo { get; set; } // for browsing/reference purposes
        public int Index { get; set; }

        public MCMloPortalDef() { }
        public MCMloPortalDef(Meta meta, CMloPortalDef data)
        {
            _Data = data;
            Corners = MetaTypes.GetPaddedVector3Array(meta, in _Data.corners);
            AttachedObjects = MetaTypes.GetUintArray(meta, in _Data.attachedObjects);
        }

        public override void Load(Meta meta, in MetaPOINTER ptr)
        {
            MetaTypes.TryGetData<CMloPortalDef>(meta, in ptr, out _Data);
            Corners = MetaTypes.GetPaddedVector3Array(meta, in _Data.corners);
            AttachedObjects = MetaTypes.GetUintArray(meta, in _Data.attachedObjects);
        }

        public override MetaPOINTER Save(MetaBuilder mb)
        {
            if (Corners is not null)
            {
                _Data.corners = mb.AddPaddedVector3ArrayPtr(Corners);
            }
            else
            {
                _Data.corners = new Array_Vector3();
            }

            if (AttachedObjects is not null)
            {
                _Data.attachedObjects = mb.AddUintArrayPtr(AttachedObjects);
            }
            else
            {
                _Data.attachedObjects = new Array_uint();
            }

            mb.AddStructureInfo(MetaName.CMloPortalDef);
            return mb.AddItemPtr(MetaName.CMloPortalDef, in _Data);
        }

        public override string Name => $"{Index}: {_Data.roomFrom} to {_Data.roomTo}";

        public override string ToString() => Name;
    }

    [TC(typeof(EXP))]
    public struct CMloEntitySet //48 bytes, Key:4180211587
    {
        public uint Unused0 { get; set; }//0
        public uint Unused1 { get; set; }//4
        public MetaHash name { get; set; } //8   8: Hash: 0: name
        public uint Unused2 { get; set; }//12
        public Array_uint locations; //16   16: Array: 0: locations  {0: UnsignedInt: 0: 256}
        public Array_StructurePointer entities; //32   32: Array: 0: entities  {0: StructurePointer: 0: 256}
    }

    [TC(typeof(EXP))]
    public class MCMloEntitySet : MetaWrapper
    {
        public CMloEntitySet _Data;
        public CMloEntitySet Data => _Data;
        public uint[] Locations { get; set; }
        public MCEntityDef[] Entities { get; set; }

        public MloArchetype? OwnerMlo { get; set; } // for browsing/reference purposes
        public int Index { get; set; }

        public bool ForceVisible { get; set; } = false; //forces this entity set visible from the project window, for rendering  purpose

        public MCMloEntitySet() {
            Entities = [];
            Locations = [];
        }
        public MCMloEntitySet(Meta meta, CMloEntitySet data, MloArchetype owner)
        {
            _Data = data;
            OwnerMlo = owner;
            Load(meta);
        }

        public override void Load(Meta meta, in MetaPOINTER ptr)
        {
            MetaTypes.TryGetData<CMloEntitySet>(meta, in ptr, out _Data);
            Load(meta);
        }

        [MemberNotNull(nameof(Entities), nameof(Locations))]
        private void Load(Meta meta)
        {
            Locations = MetaTypes.GetUintArray(meta, in _Data.locations) ?? Array.Empty<uint>();

            var ents = MetaTypes.ConvertDataArray<CEntityDef>(meta, MetaName.CEntityDef, in _Data.entities);
            if (ents != null)
            {
                Entities = new MCEntityDef[ents.Length];
                for (int i = 0; i < ents.Length; i++)
                {
                    Entities[i] = new MCEntityDef(meta, ents[i]) { OwnerMlo = OwnerMlo };
                }
            }
        }


        public override MetaPOINTER Save(MetaBuilder mb)
        {
            if (Locations.Length > 0)
            {
                _Data.locations = mb.AddUintArrayPtr(Locations);
            }
            else
            {
                _Data.locations = new Array_uint();
            }

            if (Entities.Length > 0)
            {
                _Data.entities = mb.AddWrapperArrayPtr(Entities);
            }
            else
            {
                _Data.entities = new Array_StructurePointer();
            }

            mb.AddStructureInfo(MetaName.CMloEntitySet);
            return mb.AddItemPtr(MetaName.CMloEntitySet, in _Data);
        }

        public override string Name => _Data.name.ToString();
        public override string ToString() => $"{Name}: {Entities?.Length ?? 0} entities";
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
        public Array_StructurePointer entities; //96   96: Array: 0: entities  {0: StructurePointer: 0: 256}
        public Array_Structure containerLods; //112   112: Array: 0: containerLods//2935983381  {0: Structure: 372253349: 256}
        public Array_Structure boxOccluders; //128   128: Array: 0: boxOccluders//3983590932  {0: Structure: SectionUNKNOWN7: 256}
        public Array_Structure occludeModels; //144   144: Array: 0: occludeModels//2132383965  {0: Structure: SectionUNKNOWN5: 256}
        public Array_uint physicsDictionaries; //160   160: Array: 0: physicsDictionaries//949589348  {0: Hash: 0: 256}
        public rage__fwInstancedMapData instancedData; //176   176: Structure: rage__fwInstancedMapData: instancedData//2569067561
        public Array_Structure timeCycleModifiers; //224   224: Array: 0: timeCycleModifiers  {0: Structure: CTimeCycleModifier: 256}
        public Array_Structure carGenerators; //240   240: Array: 0: carGenerators//3254823756  {0: Structure: CCarGen: 256}
        public CLODLight LODLightsSOA; //256   256: Structure: CLODLight: LODLightsSOA//1774371066
        public CDistantLODLight DistantLODLightsSOA; //392   392: Structure: CDistantLODLight: DistantLODLightsSOA//2954466641
        public CBlockDesc block; //440   440: Structure: CBlockDesc//3072355914: block


        //notes:
        //CMapData.flags: 
        //  flag 1 = SCRIPTED flag  (eg destruction)
        //  flag 2 = LOD flag? reflection proxy flag?


        public override readonly string ToString() => $"{name}: {parent}";
    }

    [TC(typeof(EXP))]
    public struct CEntityDef //128 bytes, Key:1825799514
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
        public rage__eLodType lodLevel { get; set; } //84   84: IntEnum: 1264241711: lodLevel  //LODTYPES_DEPTH_
        public uint numChildren { get; set; } //88   88: UnsignedInt: 0: numChildren//2793909385
        public rage__ePriorityLevel priorityLevel { get; set; } //92   92: IntEnum: 648413703: priorityLevel//647098393
        public Array_StructurePointer extensions; //96   96: Array: 0: extensions  {0: StructurePointer: 0: 256}
        public int ambientOcclusionMultiplier { get; set; } //112   112: SignedInt: 0: ambientOcclusionMultiplier//415356295
        public int artificialAmbientOcclusion { get; set; } //116   116: SignedInt: 0: artificialAmbientOcclusion//599844163
        public uint tintValue { get; set; } //120   120: UnsignedInt: 0: tintValue//1015358759
        public uint Unused6 { get; set; }//124


        public override readonly string ToString() => $"{JenkIndex.GetString(archetypeName)}: {JenkIndex.GetString(guid)}: {position}";
    }
    [TC(typeof(EXP))] public class MCEntityDef : MetaWrapper
    {
        public CEntityDef _Data;
        public ref CEntityDef Data => ref _Data;
        public MetaWrapper[] Extensions { get; set; }


        public MloArchetype? OwnerMlo { get; set; } // for browsing/reference purposes
        public int Index { get; set; }

        public MCEntityDef(MCEntityDef copy)
        {
            if (copy != null)
                _Data = copy._Data;

            Extensions = [];
        }
        public MCEntityDef(Meta meta, CEntityDef d)
        {
            _Data = d;
            Extensions = MetaTypes.GetExtensions(meta, in _Data.extensions) ?? [];
        }
        public MCEntityDef(in CEntityDef d, MloArchetype arch)
        {
            _Data = d;
            OwnerMlo = arch;
            Extensions = [];
        }

        public override void Load(Meta meta, in MetaPOINTER ptr)
        {
            MetaTypes.TryGetData<CEntityDef>(meta, in ptr, out _Data);
            Extensions = MetaTypes.GetExtensions(meta, in _Data.extensions) ?? [];
        }

        public override MetaPOINTER Save(MetaBuilder mb)
        {
            if (Extensions.Length > 0)
            {
                _Data.extensions = mb.AddWrapperArrayPtr(Extensions);
            }
            else
            {
                _Data.extensions = new Array_StructurePointer();
            }

            mb.AddStructureInfo(MetaName.CEntityDef);
            return mb.AddItemPtr(MetaName.CEntityDef, in _Data);
        }

        public override string Name => _Data.archetypeName.ToString();

        public override string ToString() => Name;
    }

    [TC(typeof(EXP))]
    public struct BoxOccluder //16 bytes, Key:1831736438   //boxOccluders
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

    [TC(typeof(EXP))]
    public struct OccludeModel //64 bytes, Key:1172796107  //occludeModels
    {
        public Vector3 bmin; //0   0: Float_XYZ: 0: bmin
        public float Unused0 { get; set; }//12
        public Vector3 bmax; //16   16: Float_XYZ: 0: bmax
        public float Unused1 { get; set; }//28
        public uint dataSize { get; set; } //32   32: UnsignedInt: 0: dataSize//2442753371
        public uint Unused2 { get; set; }//36
        public DataBlockPointer verts; //40   40: DataBlockPointer: 2: verts
        public ushort numVertsInBytes { get; set; } //48   48: UnsignedShort: 0: numVertsInBytes
        public ushort numTris { get; set; } //50   50: UnsignedShort: 0: numTris
        public uint flags { get; set; } //52   52: UnsignedInt: 0: flags
        public uint Unused3 { get; set; }//56
        public uint Unused4 { get; set; }//60

        public readonly BoundingBox BoundingBox => new BoundingBox(bmin, bmax);
    }

    [TC(typeof(EXP))]
    public struct rage__fwInstancedMapData //48 bytes, Key:1836780118
    {
        public uint Unused0 { get; set; }//0
        public uint Unused1 { get; set; }//4
        public MetaHash ImapLink { get; set; } //8   8: Hash: 0: ImapLink//2142127586
        public uint Unused2 { get; set; }//12
        public Array_Structure PropInstanceList; //16   16: Array: 0: PropInstanceList//3551474528  {0: Structure: rage__fwPropInstanceListDef: 256}
        public Array_Structure GrassInstanceList; //32   32: Array: 0: GrassInstanceList//255292381  {0: Structure: rage__fwGrassInstanceListDef: 256}

        public override readonly string ToString() => $"{ImapLink}, {PropInstanceList.Count1} props, {GrassInstanceList.Count1} grasses";
    }

    [TC(typeof(EXP))]
    public struct rage__fwGrassInstanceListDef //96 bytes, Key:941808164  rage__fwGrassInstanceListDef//2085051229
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
        public Array_Structure InstanceList; //72   72: Array: 0: InstanceList//470289337  {0: Structure: rage__fwGrassInstanceListDef__InstanceData: 256}
        public uint Unused2 { get; set; }//88
        public uint Unused3 { get; set; }//92

        public override readonly string ToString() => $"{archetypeName} ({InstanceList.Count1} instances)";
    }

    [TC(typeof(EXP))]
    public struct rage__fwGrassInstanceListDef__InstanceData //16 bytes, Key:2740378365 rage__fwGrassInstanceListDef__InstanceData//3985044770 // Tom: Something to do with placing foliage
    {
        public ArrayOfUshorts3 Position; //0   0: ArrayOfBytes: 3: Position - Ushorts
        public byte NormalX { get; set; } //6   6: UnsignedByte: 0: NormalX//3138065392
        public byte NormalY { get; set; } //7   7: UnsignedByte: 0: NormalY//273792636
        public ArrayOfBytes3 Color; //8   8: ArrayOfBytes: 3: Color
        public byte Scale { get; set; } //11   11: UnsignedByte: 0: Scale
        public byte Ao { get; set; } //12   12: UnsignedByte: 0: Ao//2996378564
        public ArrayOfBytes3 Pad; //13   13: ArrayOfBytes: 3: Pad

        public override readonly string ToString() => $"{Position} : {Color} : {Scale}";
    }

    [TC(typeof(EXP))]
    public struct CTimeCycleModifier //64 bytes, Key:2683420777
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

        public override readonly string ToString() => $"{name}: startHour {startHour}, endHour {endHour}, range {range}, percentage {percentage}";
    }

    [TC(typeof(EXP))]
    public struct CCarGen //80 bytes, Key:2345238261
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

        public override readonly string ToString() => $"{carModel}, {position}, {popGroup}, {livery}";
    }

    [TC(typeof(EXP))]
    public struct CLODLight //136 bytes, Key:2325189228
    {
        public uint Unused0 { get; set; }//0
        public uint Unused1 { get; set; }//4
        public Array_Structure direction; //8   8: Array: 0: direction  {0: Structure: VECTOR3: 256}
        public Array_float falloff; //24   24: Array: 0: falloff  {0: Float: 0: 256}
        public Array_float falloffExponent; //40   40: Array: 0: falloffExponent  {0: Float: 0: 256}
        public Array_uint timeAndStateFlags; //56   56: Array: 0: timeAndStateFlags=3112418278  {0: UnsignedInt: 0: 256}
        public Array_uint hash; //72   72: Array: 0: hash  {0: UnsignedInt: 0: 256}
        public Array_byte coneInnerAngle; //88   88: Array: 0: coneInnerAngle//1163671864  {0: UnsignedByte: 0: 256}
        public Array_byte coneOuterAngleOrCapExt; //104   104: Array: 0: coneOuterAngleOrCapExt=3161894080  {0: UnsignedByte: 0: 256}
        public Array_byte coronaIntensity; //120   120: Array: 0: coronaIntensity//2292363771  {0: UnsignedByte: 0: 256}
    }

    [TC(typeof(EXP))]
    public struct CDistantLODLight //48 bytes, Key:2820908419
    {
        public uint Unused0 { get; set; }//0
        public uint Unused1 { get; set; }//4
        public Array_Structure position; //8   8: Array: 0: position  {0: Structure: VECTOR3: 256}
        public Array_uint RGBI; //24   24: Array: 0: RGBI  {0: UnsignedInt: 0: 256}
        public ushort numStreetLights { get; set; } //40   40: UnsignedShort: 0: numStreetLights//3708891211
        public ushort category { get; set; } //42   42: UnsignedShort: 0: category//2052871693
        public uint Unused2 { get; set; }//44
    }

    [TC(typeof(EXP))]
    public struct CBlockDesc //72 bytes, Key:2015795449
    {
        public uint version { get; set; } //0   0: UnsignedInt: 0: version
        public uint flags { get; set; } //4   4: UnsignedInt: 0: flags
        public CharPointer name; //8   8: CharPointer: 0: name
        public CharPointer exportedBy; //24   24: CharPointer: 0: exportedBy//1983184981
        public CharPointer owner; //40   40: CharPointer: 0: owner
        public CharPointer time; //56   56: CharPointer: 0: time
    }










    [TC(typeof(EXP))]
    public struct CExtensionDefParticleEffect //96 bytes, Key:466596385
    {
        public uint Unused0 { get; set; }//0
        public uint Unused1 { get; set; }//4
        public MetaHash name { get; set; } //8   8: Hash: 0: name
        public uint Unused2 { get; set; }//12
        public Vector3 offsetPosition { get; set; } //16   16: Float_XYZ: 0: offsetPosition
        public uint Unused3 { get; set; }//28
        public Vector4 offsetRotation { get; set; } //32   32: Float_XYZW: 0: offsetRotation
        public CharPointer fxName; //48   48: CharPointer: 0: fxName
        public int fxType { get; set; } //64   64: SignedInt: 0: fxType
        public int boneTag { get; set; } //68   68: SignedInt: 0: boneTag
        public float scale { get; set; } //72   72: Float: 0: scale
        public int probability { get; set; } //76   76: SignedInt: 0: probability
        public int flags { get; set; } //80   80: SignedInt: 0: flags
        public uint color { get; set; } //84   84: UnsignedInt: 0: color
        public uint Unused4 { get; set; }//88
        public uint Unused5 { get; set; }//92

        public override readonly string ToString() => $"CExtensionDefParticleEffect - {name}";
    }

    [TC(typeof(EXP))]
    public class MCExtensionDefParticleEffect : MetaWrapper
    {
        public CExtensionDefParticleEffect _Data;
        public CExtensionDefParticleEffect Data => _Data;

        public string fxName { get; set; }

        public override void Load(Meta meta, in MetaPOINTER ptr)
        {
            MetaTypes.TryGetData<CExtensionDefParticleEffect>(meta, in ptr, out _Data);
            fxName = MetaTypes.GetString(meta, in _Data.fxName);
        }

        public override MetaPOINTER Save(MetaBuilder mb)
        {
            if (fxName != null)
            {
                _Data.fxName = mb.AddStringPtr(fxName);
            }

            mb.AddStructureInfo(MetaName.CExtensionDefParticleEffect);
            return mb.AddItemPtr(MetaName.CExtensionDefParticleEffect, in _Data);
        }

        public override string Name => fxName;

        public override string ToString() => $"{_Data} - {fxName}";
    }

    [TC(typeof(EXP))]
    public struct CExtensionDefLightEffect //48 bytes, Key:2436199897
    {
        public uint Unused0 { get; set; }//0
        public uint Unused1 { get; set; }//4
        public MetaHash name { get; set; } //8   8: Hash: 0: name
        public uint Unused2 { get; set; }//12
        public Vector3 offsetPosition; //16   16: Float_XYZ: 0: offsetPosition
        public float Unused3 { get; set; }//28
        public Array_Structure instances; //32   32: Array: 0: 274177522  {0: Structure: CLightAttrDef: 256}

        public override readonly string ToString() => $"CExtensionDefLightEffect - {name}";
    }
    [TC(typeof(EXP))] public class MCExtensionDefLightEffect : MetaWrapper
    {
        public CExtensionDefLightEffect _Data;
        public CExtensionDefLightEffect Data { get { return _Data; } }

        public CLightAttrDef[] instances { get; set; }

        public override void Load(Meta meta, in MetaPOINTER ptr)
        {
            MetaTypes.TryGetData<CExtensionDefLightEffect>(meta, in ptr, out _Data);
            instances = MetaTypes.ConvertDataArray<CLightAttrDef>(meta, MetaName.CLightAttrDef, in _Data.instances);
        }

        public override MetaPOINTER Save(MetaBuilder mb)
        {
            if (instances != null)
            {
                _Data.instances = mb.AddItemArrayPtr(MetaName.CLightAttrDef, instances);
            }

            mb.AddStructureInfo(MetaName.CLightAttrDef);
            mb.AddStructureInfo(MetaName.CExtensionDefLightEffect);
            return mb.AddItemPtr(MetaName.CExtensionDefLightEffect, in _Data);
        }

        public override string Name => _Data.name.ToString();

        public override string ToString() => $"{_Data} ({instances?.Length ?? 0} Attributes)";
    }

    [TC(typeof(EXP))]
    public struct CLightAttrDef //160 bytes, Key:2363260268
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

        public override readonly string ToString() => $"CExtensionDefAudioCollisionSettings - {name}";
    }
    [TC(typeof(EXP))] public class MCExtensionDefAudioCollisionSettings : MetaWrapper
    {
        public CExtensionDefAudioCollisionSettings _Data;
        public CExtensionDefAudioCollisionSettings Data => _Data;

        public override void Load(Meta meta, in MetaPOINTER ptr)
        {
            MetaTypes.TryGetData<CExtensionDefAudioCollisionSettings>(meta, in ptr, out _Data);
        }

        public override MetaPOINTER Save(MetaBuilder mb)
        {
            mb.AddStructureInfo(MetaName.CExtensionDefAudioCollisionSettings);
            return mb.AddItemPtr(MetaName.CExtensionDefAudioCollisionSettings, in _Data);
        }

        public override string Name => _Data.name.ToString();

        public override string ToString() => _Data.ToString();
    }

    [TC(typeof(EXP))]
    public struct CExtensionDefAudioEmitter //64 bytes, Key:15929839
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

        public override readonly string ToString() => $"CExtensionDefAudioEmitter - {name}: {effectHash}: {offsetPosition}";
    }

    [TC(typeof(EXP))]
    public class MCExtensionDefAudioEmitter : MetaWrapper
    {
        public CExtensionDefAudioEmitter _Data;
        public CExtensionDefAudioEmitter Data => _Data;

        public override void Load(Meta meta, in MetaPOINTER ptr)
        {
            MetaTypes.TryGetData<CExtensionDefAudioEmitter>(meta, in ptr, out _Data);
        }

        public override MetaPOINTER Save(MetaBuilder mb)
        {
            mb.AddStructureInfo(MetaName.CExtensionDefAudioEmitter);
            return mb.AddItemPtr(MetaName.CExtensionDefAudioEmitter, in _Data);
        }

        public override string Name => _Data.name.ToString();

        public override string ToString() => _Data.ToString();
    }

    [TC(typeof(EXP))]
    public struct CExtensionDefExplosionEffect //80 bytes, Key:2840366784
    {
        public uint Unused0 { get; set; }//0
        public uint Unused1 { get; set; }//4
        public MetaHash name { get; set; } //8   8: Hash: 0: name
        public uint Unused2 { get; set; }//12
        public Vector3 offsetPosition { get; set; } //16   16: Float_XYZ: 0: offsetPosition
        public float Unused3 { get; set; }//28
        public Vector4 offsetRotation { get; set; } //32   32: Float_XYZW: 0: offsetRotation
        public CharPointer explosionName; //48   48: CharPointer: 0: explosionName//3301388915
        public int boneTag { get; set; } //64   64: SignedInt: 0: boneTag
        public int explosionTag { get; set; } //68   68: SignedInt: 0: explosionTag//2653034051
        public int explosionType { get; set; } //72   72: SignedInt: 0: explosionType//3379115010
        public uint flags { get; set; } //76   76: UnsignedInt: 0: flags

        public override readonly string ToString() => $"CExtensionDefExplosionEffect - {name}";
    }
    [TC(typeof(EXP))] public class MCExtensionDefExplosionEffect : MetaWrapper
    {
        public CExtensionDefExplosionEffect _Data;
        public CExtensionDefExplosionEffect Data => _Data;

        public string explosionName { get; set; }

        public override void Load(Meta meta, in MetaPOINTER ptr)
        {
            MetaTypes.TryGetData<CExtensionDefExplosionEffect>(meta, in ptr, out _Data);
            explosionName = MetaTypes.GetString(meta, in _Data.explosionName);
        }

        public override MetaPOINTER Save(MetaBuilder mb)
        {
            if (explosionName != null)
            {
                _Data.explosionName = mb.AddStringPtr(explosionName);
            }
            mb.AddStructureInfo(MetaName.CExtensionDefExplosionEffect);
            return mb.AddItemPtr(MetaName.CExtensionDefExplosionEffect, in _Data);
        }

        public override string Name => _Data.name.ToString();

        public override string ToString() => $"{_Data} - {explosionName}";
    }

    [TC(typeof(EXP))]
    public struct CExtensionDefLadder //96 bytes, Key:1978210597
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
        public CExtensionDefLadderMaterialType materialType { get; set; } //80   80: IntEnum: CExtensionDefLadderMaterialType: materialType
        public MetaHash template { get; set; } //84   84: Hash: 0: template
        public byte canGetOffAtTop { get; set; } //88   88: Boolean: 0: canGetOffAtTop
        public byte canGetOffAtBottom { get; set; } //89   89: Boolean: 0: canGetOffAtBottom
        public ushort Unused7 { get; set; }//90
        public uint Unused8 { get; set; }//92

        public override readonly string ToString() => $"CExtensionDefLadder - {name}";
    }

    [TC(typeof(EXP))]
    public class MCExtensionDefLadder : MetaWrapper
    {
        public CExtensionDefLadder _Data;
        public CExtensionDefLadder Data => _Data;

        public override void Load(Meta meta, in MetaPOINTER ptr)
        {
            MetaTypes.TryGetData<CExtensionDefLadder>(meta, in ptr, out _Data);
        }

        public override MetaPOINTER Save(MetaBuilder mb)
        {
            mb.AddEnumInfo(MetaName.CExtensionDefLadderMaterialType);
            mb.AddStructureInfo(MetaName.CExtensionDefLadder);
            return mb.AddItemPtr(MetaName.CExtensionDefLadder, in _Data);
        }

        public override string Name => _Data.name.ToString();
        public override string ToString() => _Data.ToString();
    }

    [TC(typeof(EXP))]
    public struct CExtensionDefBuoyancy //32 bytes, Key:2383039928
    {
        public uint Unused0 { get; set; }//0
        public uint Unused1 { get; set; }//4
        public MetaHash name { get; set; } //8   8: Hash: 0: name
        public uint Unused2 { get; set; }//12
        public Vector3 offsetPosition { get; set; } //16   16: Float_XYZ: 0: offsetPosition
        public float Unused3 { get; set; }//28

        public override string ToString()
        {
            return $"CExtensionDefBuoyancy - {name}";
        }
    }
    [TC(typeof(EXP))] public class MCExtensionDefBuoyancy : MetaWrapper
    {
        public CExtensionDefBuoyancy _Data;
        public CExtensionDefBuoyancy Data => _Data;

        public override void Load(Meta meta, in MetaPOINTER ptr)
        {
            MetaTypes.TryGetData<CExtensionDefBuoyancy>(meta, in ptr, out _Data);
        }

        public override MetaPOINTER Save(MetaBuilder mb)
        {
            mb.AddStructureInfo(MetaName.CExtensionDefBuoyancy);
            return mb.AddItemPtr(MetaName.CExtensionDefBuoyancy, in _Data);
        }

        public override string Name => _Data.name.ToString();
        public override string ToString() => _Data.ToString();
    }

    [TC(typeof(EXP))]
    public struct CExtensionDefExpression //48 bytes, Key:24441706
    {
        public uint Unused0 { get; set; }//0
        public uint Unused1 { get; set; }//4
        public MetaHash name { get; set; } //8   8: Hash: 0: name
        public uint Unused2 { get; set; }//12
        public Vector3 offsetPosition { get; set; } //16   16: Float_XYZ: 0: offsetPosition
        public float Unused3 { get; set; }//28
        public MetaHash expressionDictionaryName { get; set; } //32   32: Hash: 0: expressionDictionaryName
        public MetaHash expressionName { get; set; } //36   36: Hash: 0: expressionName
        public MetaHash creatureMetadataName { get; set; } //40   40: Hash: 0: creatureMetadataName
        public byte initialiseOnCollision { get; set; } //44   44: Boolean: 0: initialiseOnCollision
        public byte Unused4 { get; set; }//45
        public ushort Unused5 { get; set; }//46

        public override readonly string ToString() => $"CExtensionDefExpression - {name}";
    }

    [TC(typeof(EXP))]
    public class MCExtensionDefExpression : MetaWrapper
    {
        public CExtensionDefExpression _Data;
        public CExtensionDefExpression Data { get { return _Data; } }

        public override void Load(Meta meta, in MetaPOINTER ptr)
        {
            MetaTypes.TryGetData<CExtensionDefExpression>(meta, in ptr, out _Data);
        }

        public override MetaPOINTER Save(MetaBuilder mb)
        {
            mb.AddStructureInfo(MetaName.CExtensionDefExpression);
            return mb.AddItemPtr(MetaName.CExtensionDefExpression, in _Data);
        }

        public override string Name => _Data.name.ToString();

        public override string ToString() => _Data.ToString();
    }

    [TC(typeof(EXP))]
    public struct CExtensionDefLightShaft //176 bytes, Key:2526429398
    {
        public uint Unused00 { get; set; }//0
        public uint Unused01 { get; set; }//4
        public MetaHash name { get; set; } //8   8: Hash: 0: name
        public uint Unused02 { get; set; }//12
        public Vector3 offsetPosition { get; set; } //16   16: Float_XYZ: 0: offsetPosition
        public float Unused03 { get; set; }//28
        public Vector3 cornerA { get; set; } //32   32: Float_XYZ: 0: cornerA
        public float Unused04 { get; set; }//44
        public Vector3 cornerB { get; set; } //48   48: Float_XYZ: 0: cornerB
        public float Unused05 { get; set; }//60
        public Vector3 cornerC { get; set; } //64   64: Float_XYZ: 0: cornerC
        public float Unused06 { get; set; }//76
        public Vector3 cornerD { get; set; } //80   80: Float_XYZ: 0: cornerD
        public float Unused07 { get; set; }//92
        public Vector3 direction { get; set; } //96   96: Float_XYZ: 0: direction
        public float Unused08 { get; set; }//108
        public float directionAmount { get; set; } //112   112: Float: 0: directionAmount
        public float length { get; set; } //116   116: Float: 0: length
        public float fadeInTimeStart { get; set; } //120   120: Float: 0: fadeInTimeStart
        public float fadeInTimeEnd { get; set; } //124   124: Float: 0: fadeInTimeEnd
        public float fadeOutTimeStart { get; set; } //128   128: Float: 0: fadeOutTimeStart
        public float fadeOutTimeEnd { get; set; } //132   132: Float: 0: fadeOutTimeEnd
        public float fadeDistanceStart { get; set; } //136   136: Float: 0: fadeDistanceStart
        public float fadeDistanceEnd { get; set; } //140   140: Float: 0: fadeDistanceEnd
        public uint color { get; set; } //144   144: UnsignedInt: 0: color
        public float intensity { get; set; } //148   148: Float: 0: intensity
        public byte flashiness { get; set; } //152   152: UnsignedByte: 0: flashiness
        public byte Unused09 { get; set; }//153
        public ushort Unused10 { get; set; }//154
        public uint flags { get; set; } //156   156: UnsignedInt: 0: flags
        public CExtensionDefLightShaftDensityType densityType { get; set; } //160   160: IntEnum: CExtensionDefLightShaftDensityType: densityType
        public CExtensionDefLightShaftVolumeType volumeType { get; set; } //164   164: IntEnum: CExtensionDefLightShaftVolumeType: volumeType
        public float softness { get; set; } //168   168: Float: 0: softness
        public byte scaleBySunIntensity { get; set; } //172   172: Boolean: 0: scaleBySunIntensity
        public byte Unused11 { get; set; }//173
        public ushort Unused12 { get; set; }//174

        public override readonly string ToString()
        {
            return $"CExtensionDefLightShaft - {name}";
        }
    }
    [TC(typeof(EXP))]
    public class MCExtensionDefLightShaft : MetaWrapper
    {
        public CExtensionDefLightShaft _Data;
        public CExtensionDefLightShaft Data => _Data;

        public override void Load(Meta meta, in MetaPOINTER ptr)
        {
            MetaTypes.TryGetData<CExtensionDefLightShaft>(meta, in ptr, out _Data);
        }

        public override MetaPOINTER Save(MetaBuilder mb)
        {
            mb.AddEnumInfo(MetaName.CExtensionDefLightShaftDensityType);
            mb.AddEnumInfo(MetaName.CExtensionDefLightShaftVolumeType);
            mb.AddStructureInfo(MetaName.CExtensionDefLightShaft);
            return mb.AddItemPtr(MetaName.CExtensionDefLightShaft, in _Data);
        }

        public override string Name => _Data.name.ToString();
        public override string ToString() => _Data.ToString();
    }

    [TC(typeof(EXP))]
    public struct CExtensionDefDoor //48 bytes, Key:2671601385
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

        public override readonly string ToString()
        {
            return $"CExtensionDefDoor - {name}, {audioHash}, {offsetPosition}, {limitAngle}";
        }
    }
    [TC(typeof(EXP))]
    public class MCExtensionDefDoor : MetaWrapper
    {
        public CExtensionDefDoor _Data;
        public CExtensionDefDoor Data => _Data;

        public override void Load(Meta meta, in MetaPOINTER ptr)
        {
            MetaTypes.TryGetData<CExtensionDefDoor>(meta, in ptr, out _Data);
        }

        public override MetaPOINTER Save(MetaBuilder mb)
        {
            mb.AddStructureInfo(MetaName.CExtensionDefDoor);
            return mb.AddItemPtr(MetaName.CExtensionDefDoor, in _Data);
        }

        public override string Name => _Data.name.ToString();

        public override string ToString() => _Data.ToString();
    }

    [TC(typeof(EXP))]
    public struct CExtensionDefSpawnPoint //96 bytes, Key:3077340721
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
        public CSpawnPoint__AvailabilityMpSp availableInMpSp { get; set; } //68   68: IntEnum: CSpawnPoint__AvailabilityMpSp: availableInMpSp
        public float probability { get; set; } //72   72: Float: 0: probability
        public float timeTillPedLeaves { get; set; } //76   76: Float: 0: timeTillPedLeaves
        public float radius { get; set; } //80   80: Float: 0: radius
        public byte start { get; set; } //84   84: UnsignedByte: 0: start
        public byte end { get; set; } //85   85: UnsignedByte: 0: end
        public ushort Unused4 { get; set; }//86
        public CScenarioPointFlags__Flags flags { get; set; } //88   88: IntFlags2: 700327466: flags
        public byte highPri { get; set; } //92   92: Boolean: 0: highPri
        public byte extendedRange { get; set; } //93   93: Boolean: 0: extendedRange
        public byte shortRange { get; set; } //94   94: Boolean: 0: shortRange
        public byte Unused5 { get; set; }//95

        public override readonly string ToString()
        {
            return $"{spawnType}: {name}, {pedType}";// + ", " + flags.ToString() + ", " + offsetPosition.ToString();
        }
    }
    [TC(typeof(EXP))]
    public class MCExtensionDefSpawnPoint : MetaWrapper
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
        public CSpawnPoint__AvailabilityMpSp AvailableInMpSp { get { return _Data.availableInMpSp; } set { _Data.availableInMpSp = value; } }
        public float Probability { get { return _Data.probability; } set { _Data.probability = value; } }
        public float TimeTillPedLeaves { get { return _Data.timeTillPedLeaves; } set { _Data.timeTillPedLeaves = value; } }
        public float Radius { get { return _Data.radius; } set { _Data.radius = value; } }
        public byte StartTime { get { return _Data.start; } set { _Data.start = value; } }
        public byte EndTime { get { return _Data.end; } set { _Data.end = value; } }
        public CScenarioPointFlags__Flags Flags { get { return _Data.flags; } set { _Data.flags = value; } }
        public bool HighPri { get { return _Data.highPri == 1; } set { _Data.highPri = (byte)(value ? 1 : 0); } }
        public bool ExtendedRange { get { return _Data.extendedRange == 1; } set { _Data.extendedRange = (byte)(value ? 1 : 0); } }
        public bool ShortRange { get { return _Data.shortRange == 1; } set { _Data.shortRange = (byte)(value ? 1 : 0); } }

        public Vector3 Position { get { return _Data.offsetPosition + ParentPosition; } set { _Data.offsetPosition = value - ParentPosition; } }
        public Quaternion Orientation { get { return new Quaternion(_Data.offsetRotation);  } set { _Data.offsetRotation = value.ToVector4(); } }

        public MCExtensionDefSpawnPoint() { }
        public MCExtensionDefSpawnPoint(MCScenarioPointRegion region, Meta meta, in CExtensionDefSpawnPoint data, object parent)
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

        public override void Load(Meta meta, in MetaPOINTER ptr)
        {
            MetaTypes.TryGetData<CExtensionDefSpawnPoint>(meta, in ptr, out _Data);
        }

        public override MetaPOINTER Save(MetaBuilder mb)
        {
            mb.AddEnumInfo(MetaName.CSpawnPoint__AvailabilityMpSp);
            mb.AddEnumInfo(MetaName.CScenarioPointFlags__Flags);
            mb.AddStructureInfo(MetaName.CExtensionDefSpawnPoint);
            return mb.AddItemPtr(MetaName.CExtensionDefSpawnPoint, in _Data);
        }

        public override string Name => _Data.name.ToString();
        public override string ToString() => _Data.ToString();
    }

    [TC(typeof(EXP))]
    public struct CExtensionDefSpawnPointOverride //64 bytes, Key:2551875873
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
        public CSpawnPoint__AvailabilityMpSp AvailabilityInMpSp { get; set; } //48   48: IntEnum: CSpawnPoint__AvailabilityMpSp: AvailabilityInMpSp//2932681318
        public CScenarioPointFlags__Flags Flags { get; set; } //52   52: IntFlags2: 700327466: Flags
        public float Radius { get; set; } //56   56: Float: 0: Radius
        public float TimeTillPedLeaves { get; set; } //60   60: Float: 0: TimeTillPedLeaves//4073598194

        public override readonly string ToString() => $"CExtensionDefSpawnPointOverride - {name}";
    }

    [TC(typeof(EXP))]
    public class MCExtensionDefSpawnPointOverride : MetaWrapper
    {
        public CExtensionDefSpawnPointOverride _Data;
        public CExtensionDefSpawnPointOverride Data => _Data;

        public override void Load(Meta meta, in MetaPOINTER ptr)
        {
            MetaTypes.TryGetData<CExtensionDefSpawnPointOverride>(meta, in ptr, out _Data);
        }

        public override MetaPOINTER Save(MetaBuilder mb)
        {
            mb.AddEnumInfo(MetaName.CSpawnPoint__AvailabilityMpSp);
            mb.AddEnumInfo(MetaName.CScenarioPointFlags__Flags);
            mb.AddStructureInfo(MetaName.CExtensionDefSpawnPointOverride);
            return mb.AddItemPtr(MetaName.CExtensionDefSpawnPointOverride, in _Data);
        }

        public override string Name => _Data.name.ToString();

        public override string ToString() => _Data.ToString();
    }

    [TC(typeof(EXP))]
    public struct CExtensionDefWindDisturbance //96 bytes, Key:3971538917
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

        public override readonly string ToString() => $"CExtensionDefWindDisturbance - {name}";
    }
    [TC(typeof(EXP))]
    public class MCExtensionDefWindDisturbance : MetaWrapper
    {
        public CExtensionDefWindDisturbance _Data;
        public CExtensionDefWindDisturbance Data => _Data;

        public override void Load(Meta meta, in MetaPOINTER ptr)
        {
            MetaTypes.TryGetData<CExtensionDefWindDisturbance>(meta, in ptr, out _Data);
        }

        public override MetaPOINTER Save(MetaBuilder mb)
        {
            mb.AddStructureInfo(MetaName.CExtensionDefWindDisturbance);
            return mb.AddItemPtr(MetaName.CExtensionDefWindDisturbance, in _Data);
        }

        public override string Name => _Data.name.ToString();

        public override string ToString() => _Data.ToString();
    }

    [TC(typeof(EXP))]
    public struct CExtensionDefProcObject //80 bytes, Key:3965391891
    {
        public uint Unused0 { get; set; }//0
        public uint Unused1 { get; set; }//4
        public MetaHash name { get; set; } //8   8: Hash: 0: name
        public uint Unused2 { get; set; }//12
        public Vector3 offsetPosition { get; set; } //16   16: Float_XYZ: 0: offsetPosition
        public float Unused3 { get; set; }//28
        public float radiusInner { get; set; } //32   32: Float: 0: radiusInner
        public float radiusOuter { get; set; } //36   36: Float: 0: radiusOuter
        public float spacing { get; set; } //40   40: Float: 0: spacing
        public float minScale { get; set; } //44   44: Float: 0: minScale
        public float maxScale { get; set; } //48   48: Float: 0: maxScale
        public float minScaleZ { get; set; } //52   52: Float: 0: minScaleZ
        public float maxScaleZ { get; set; } //56   56: Float: 0: maxScaleZ
        public float minZOffset { get; set; } //60   60: Float: 0: minZOffset
        public float maxZOffset { get; set; } //64   64: Float: 0: maxZOffset
        public uint objectHash { get; set; } //68   68: UnsignedInt: 0: objectHash
        public uint flags { get; set; } //72   72: UnsignedInt: 0: flags
        public uint Unused4 { get; set; }//76

        public override readonly string ToString() => $"CExtensionDefProcObject - {name}";
    }

    [TC(typeof(EXP))] public class MCExtensionDefProcObject : MetaWrapper
    {
        public CExtensionDefProcObject _Data;
        public CExtensionDefProcObject Data { get { return _Data; } }

        public override void Load(Meta meta, in MetaPOINTER ptr)
        {
            MetaTypes.TryGetData<CExtensionDefProcObject>(meta, in ptr, out _Data);
        }

        public override MetaPOINTER Save(MetaBuilder mb)
        {
            mb.AddStructureInfo(MetaName.CExtensionDefProcObject);
            return mb.AddItemPtr(MetaName.CExtensionDefProcObject, in _Data);
        }

        public override string Name => _Data.name.ToString();

        public override string ToString() => _Data.ToString();
    }

    [TC(typeof(EXP))] public struct rage__phVerletClothCustomBounds //32 bytes, Key:2075461750
    {
        public uint Unused0 { get; set; }//0
        public uint Unused1 { get; set; }//4
        public MetaHash name { get; set; } //8   8: Hash: 0: name
        public uint Unused2 { get; set; }//12
        public Array_Structure CollisionData; //16   16: Array: 0: CollisionData  {0: Structure: SectionUNKNOWN1: 256}
    }

    [TC(typeof(EXP))] public class Mrage__phVerletClothCustomBounds : MetaWrapper
    {
        public rage__phVerletClothCustomBounds _Data;
        public rage__phVerletClothCustomBounds Data { get { return _Data; } }

        public Mrage__phCapsuleBoundDef[] CollisionData { get; set; }

        public override void Load(Meta meta, in MetaPOINTER ptr)
        {
            MetaTypes.TryGetData<rage__phVerletClothCustomBounds>(meta, in ptr, out _Data);

            var cdata = MetaTypes.ConvertDataArray<rage__phCapsuleBoundDef>(meta, MetaName.rage__phCapsuleBoundDef, in _Data.CollisionData);
            if (cdata != null)
            {
                CollisionData = new Mrage__phCapsuleBoundDef[cdata.Length];
                for (int i = 0; i < cdata.Length; i++)
                {
                    CollisionData[i] = new Mrage__phCapsuleBoundDef(meta, in cdata[i]);
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
            return mb.AddItemPtr(MetaName.rage__phVerletClothCustomBounds, in _Data);
        }

        public override string ToString() => $"rage__phVerletClothCustomBounds - {_Data.name} ({CollisionData?.Length ?? 0} CollisionData)";
    }

    [TC(typeof(EXP))] public struct rage__phCapsuleBoundDef //96 bytes, Key:2859775340 //dexy: cloth CollisionData (child of rage__phVerletClothCustomBounds) ... eg josh house  // Tom: explosions? 
    {
        public CharPointer OwnerName; //0   0: CharPointer: 0: OwnerName
        public Vector4 Rotation { get; set; } //16   16: Float_XYZW: 0: Rotation
        public Vector3 Position { get; set; } //32   32: Float_XYZ: 0: Position
        public float Unused0 { get; set; }//44
        public Vector3 Normal { get; set; } //48   48: Float_XYZ: 0: Normal
        public float Unused1 { get; set; }//60
        public float CapsuleRadius { get; set; } //64   64: Float: 0: CapsuleRadius
        public float CapsuleLen { get; set; } //68   68: Float: 0: CapsuleLen
        public float CapsuleHalfHeight { get; set; } //72   72: Float: 0: CapsuleHalfHeight
        public float CapsuleHalfWidth { get; set; } //76   76: Float: 0: CapsuleHalfWidth
        public rage__phCapsuleBoundDef__enCollisionBoundDef Flags { get; set; } //80   80: IntFlags2: rage__phCapsuleBoundDef__enCollisionBoundDef: Flags
        public uint Unused2 { get; set; }//84
        public uint Unused3 { get; set; }//88
        public uint Unused4 { get; set; }//92
    }

    [TC(typeof(EXP))]
    public class Mrage__phCapsuleBoundDef : MetaWrapper
    {
        public rage__phCapsuleBoundDef _Data;
        public rage__phCapsuleBoundDef Data => _Data;

        public string OwnerName { get; set; }

        public Mrage__phCapsuleBoundDef() { }
        public Mrage__phCapsuleBoundDef(Meta meta, in rage__phCapsuleBoundDef s)
        {
            _Data = s;
            OwnerName = MetaTypes.GetString(meta, in _Data.OwnerName);
        }
        public override void Load(Meta meta, in MetaPOINTER ptr)
        {
            MetaTypes.TryGetData<rage__phCapsuleBoundDef>(meta, in ptr, out _Data);
            OwnerName = MetaTypes.GetString(meta, in _Data.OwnerName);
        }

        public override MetaPOINTER Save(MetaBuilder mb)
        {
            if (OwnerName != null)
            {
                _Data.OwnerName = mb.AddStringPtr(OwnerName);
            }

            mb.AddEnumInfo(MetaName.rage__phCapsuleBoundDef__enCollisionBoundDef);
            mb.AddStructureInfo(MetaName.rage__phCapsuleBoundDef);
            return mb.AddItemPtr(MetaName.rage__phCapsuleBoundDef, in _Data);
        }

        public override string ToString()
        {
            return $"rage__phCapsuleBoundDef - {OwnerName}";
        }
    }










    [TC(typeof(EXP))] public struct rage__spdGrid2D //64 bytes, Key:894636096
    {
        public uint Unused0 { get; set; }//0
        public uint Unused1 { get; set; }//4
        public uint Unused2 { get; set; }//8
        public int MinCellX { get; set; } //12   12: SignedInt: 0: MinCellX   //MIN X
        public int MaxCellX { get; set; } //16   16: SignedInt: 0: MaxCellX //MAX X
        public int MinCellY { get; set; } //20   20: SignedInt: 0: MinCellY   //MIN Y
        public int MaxCellY { get; set; } //24   24: SignedInt: 0: MaxCellY //MAX Y
        public uint Unused3 { get; set; }//28
        public uint Unused4 { get; set; }//32
        public uint Unused5 { get; set; }//36
        public uint Unused6 { get; set; }//40
        public float CellDimX { get; set; } //44   44: Float: 0: CellDimX //grid scale X (cell size)
        public float CellDimY { get; set; } //48   48: Float: 0: CellDimY //grid scale Y (cell size)
        public uint Unused7 { get; set; }//52
        public uint Unused8 { get; set; }//56
        public uint Unused9 { get; set; }//60


        public readonly Vector2I Dimensions => new Vector2I((MaxCellX - MinCellX) + 1, (MaxCellY - MinCellY) + 1);
        public Vector2 Scale
        {
            get => new Vector2(CellDimX, CellDimY);
            set
            {
                CellDimX = value.X;
                CellDimY = value.Y;
            }
        }
        public Vector2 Min
        {
            get => new Vector2(MinCellX, MinCellY) * Scale;
            set
            {
                var gv = value / Scale;
                MinCellX = (int)Math.Floor(gv.X);
                MinCellY = (int)Math.Floor(gv.Y);
            }
        }
        public Vector2 Max
        {
            get => new Vector2(MaxCellX, MaxCellY) * Scale;
            set
            {
                var gv = value / Scale;
                MaxCellX = (int)Math.Floor(gv.X);
                MaxCellY = (int)Math.Floor(gv.Y);
            }
        }

        public static bool operator ==(rage__spdGrid2D x, rage__spdGrid2D y)
        {
            return x.Equals(y);
        }

        public static bool operator !=(rage__spdGrid2D x, rage__spdGrid2D y)
        {
            return x.Equals(y);
        }

        public override readonly bool Equals(object obj)
        {
            return obj is rage__spdGrid2D arrObj
                && arrObj.MaxCellX == this.MaxCellX
                && arrObj.MaxCellY == this.MaxCellY
                && arrObj.MinCellX == this.MinCellX
                && arrObj.MinCellY == this.MinCellY
                && arrObj.CellDimX == this.CellDimX
                && arrObj.CellDimY == this.CellDimY
                && arrObj.Unused0 == this.Unused0
                && arrObj.Unused1 == this.Unused1
                && arrObj.Unused2 == this.Unused2
                && arrObj.Unused3 == this.Unused3
                && arrObj.Unused4 == this.Unused4
                && arrObj.Unused5 == this.Unused5
                && arrObj.Unused6 == this.Unused6
                && arrObj.Unused7 == this.Unused7
                && arrObj.Unused8 == this.Unused8
                && arrObj.Unused9 == this.Unused9;
        }

        public override readonly int GetHashCode()
        {
            int hashCode = 418850833;
            hashCode = hashCode * -1521134295 + Unused0.GetHashCode();
            hashCode = hashCode * -1521134295 + Unused1.GetHashCode();
            hashCode = hashCode * -1521134295 + Unused2.GetHashCode();
            hashCode = hashCode * -1521134295 + MinCellX.GetHashCode();
            hashCode = hashCode * -1521134295 + MaxCellX.GetHashCode();
            hashCode = hashCode * -1521134295 + MinCellY.GetHashCode();
            hashCode = hashCode * -1521134295 + MaxCellY.GetHashCode();
            hashCode = hashCode * -1521134295 + Unused3.GetHashCode();
            hashCode = hashCode * -1521134295 + Unused4.GetHashCode();
            hashCode = hashCode * -1521134295 + Unused5.GetHashCode();
            hashCode = hashCode * -1521134295 + Unused6.GetHashCode();
            hashCode = hashCode * -1521134295 + CellDimX.GetHashCode();
            hashCode = hashCode * -1521134295 + CellDimY.GetHashCode();
            hashCode = hashCode * -1521134295 + Unused7.GetHashCode();
            hashCode = hashCode * -1521134295 + Unused8.GetHashCode();
            hashCode = hashCode * -1521134295 + Unused9.GetHashCode();
            return hashCode;
        }
    }

    [TC(typeof(EXP))]
    public struct rage__spdAABB //32 bytes, Key:1158138379
    {
        public Vector4 min { get; set; } //0   0: Float_XYZW: 0: min
        public Vector4 max { get; set; } //16   16: Float_XYZW: 0: max

        public rage__spdAABB(in Vector3 _min, in Vector3 _max)
        {
            min = new Vector4(_min.X, _min.Y, _min.Z, 0);
            max = new Vector4(_max.X, _max.Y, _max.Z, 0);
        }

        public readonly override string ToString()
        {
            return $"min: {min}, max: {max}";
        }
        public readonly rage__spdAABB SwapEnd()
        {
            return this with
            {
                min = MetaTypes.SwapBytes(min),
                max = MetaTypes.SwapBytes(max),
            };

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





    [TC(typeof(EXP))]
    public struct CScenarioPointRegion  //SCENARIO YMT ROOT  - in /scenario/ folder //376 bytes, Key:3501351821
    {
        public int VersionNumber { get; set; } //0   0: SignedInt: 0: VersionNumber
        public uint Unused0 { get; set; }//4
        public CScenarioPointContainer Points; //8   8: Structure: CScenarioPointContainer//2380938603: Points//702683191
        public uint Unused1 { get; set; }//56
        public uint Unused2 { get; set; }//60
        public uint Unused3 { get; set; }//64
        public uint Unused4 { get; set; }//68
        public Array_Structure EntityOverrides; //72   72: Array: 0: EntityOverrides//697469539  {0: Structure: CScenarioEntityOverride//4213733800: 256}
        public uint Unused5 { get; set; }//88
        public uint Unused6 { get; set; }//92
        public CScenarioChainingGraph ChainingGraph; //[PATHS] 96   96: Structure: CScenarioChainingGraph: ChainingGraph
        public rage__spdGrid2D AccelGrid; //184   184: Structure: rage__spdGrid2D: AccelGrid//3053155275
        public Array_ushort Unk_3844724227; //248   248: Array: 0: 3844724227  {0: UnsignedShort: 0: 256}
        public Array_Structure Clusters; //264   264: Array: 0: Clusters//3587988394  {0: Structure: CScenarioPointCluster//750308016: 256}
        public CScenarioPointLookUps LookUps; //280   280: Structure: CScenarioPointLookUps//3019621867: LookUps//1097626284
    }

    [TC(typeof(EXP))]
    public class MCScenarioPointRegion : MetaWrapper
    {
        public YmtFile Ymt { get; set; }

        public CScenarioPointRegion _Data;
        public CScenarioPointRegion Data { get { return _Data; } }

        public MCScenarioPointContainer Points { get; set; }
        public MCScenarioEntityOverride[] EntityOverrides { get; set; }
        public MCScenarioChainingGraph Paths { get; set; }
        public ushort[] Unk_3844724227 { get; set; } //GRID DATA - 2d dimensions - AccelGrid ((MaxX-MinX)+1)*((MaxY-MinY)+1)
        public MCScenarioPointCluster[] Clusters { get; set; }
        public MCScenarioPointLookUps LookUps { get; set; }

        public int VersionNumber { get { return _Data.VersionNumber; } set { _Data.VersionNumber = value; } }

        public override void Load(Meta meta, in MetaPOINTER ptr)
        {
            MetaTypes.TryGetData<CScenarioPointRegion>(meta, in ptr, out var data);
            Load(meta, in data);
        }
        public void Load(Meta meta, in CScenarioPointRegion data)
        {
            _Data = data;


            Points = new MCScenarioPointContainer(this, meta, in _Data.Points);


            var entOverrides = MetaTypes.ConvertDataArray<CScenarioEntityOverride>(meta, MetaName.CScenarioEntityOverride, in _Data.EntityOverrides);
            if (entOverrides != null)
            {
                EntityOverrides = new MCScenarioEntityOverride[entOverrides.Length];
                for (int i = 0; i < entOverrides.Length; i++)
                {
                    EntityOverrides[i] = new MCScenarioEntityOverride(this, meta, in entOverrides[i]);
                }
            }


            Paths = new MCScenarioChainingGraph(this, meta, _Data.ChainingGraph);


            var clusters = MetaTypes.ConvertDataArray<CScenarioPointCluster>(meta, MetaName.CScenarioPointCluster, in _Data.Clusters);
            if (clusters != null)
            {
                Clusters = new MCScenarioPointCluster[clusters.Length];
                for (int i = 0; i < clusters.Length; i++)
                {
                    Clusters[i] = new MCScenarioPointCluster(this, meta, clusters[i]);
                }
            }

            Unk_3844724227 = MetaTypes.GetUshortArray(meta, in _Data.Unk_3844724227);

            LookUps = new MCScenarioPointLookUps(this, meta, in _Data.LookUps);


            #region data analysis
            ////data analysis
            //if (Points.LoadSavePoints != null)
            //{ } //no hits here!
            //if (Unk_3844724227 != null)
            //{
            //    var grid = _Data.AccelGrid;
            //    var minx = grid.MinCellX;
            //    var maxx = grid.MaxCellX;
            //    var miny = grid.MinCellY;
            //    var maxy = grid.MaxCellY;
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
            //var hashb = mb.EnsureBlock((MetaName)MetaTypeName.HASH);
            //var ushb = mb.EnsureBlock((MetaName)MetaTypeName.USHORT);
            //var pntb = mb.EnsureBlock(MetaName.CScenarioPoint);

            mb.AddStructureInfo(MetaName.CScenarioPointContainer);
            mb.AddStructureInfo(MetaName.CScenarioChainingGraph);
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
                    mb.AddEnumInfo(MetaName.CSpawnPoint__AvailabilityMpSp);
                    mb.AddEnumInfo(MetaName.CScenarioPointFlags__Flags);
                    scp.LoadSavePoints = mb.AddItemArrayPtr(MetaName.CExtensionDefSpawnPoint, loadSavePoints);
                }
                var myPoints = Points.GetCMyPoints();
                if (myPoints != null)
                {
                    mb.AddStructureInfo(MetaName.CScenarioPoint);
                    mb.AddEnumInfo(MetaName.CScenarioPointFlags__Flags);
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
                    var cent = mcent._Data;
                    var scps = mcent.GetCScenarioPoints();
                    if (scps != null)
                    {
                        mb.AddStructureInfo(MetaName.CExtensionDefSpawnPoint);
                        mb.AddEnumInfo(MetaName.CSpawnPoint__AvailabilityMpSp);
                        mb.AddEnumInfo(MetaName.CScenarioPointFlags__Flags);
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


            if (Paths is not null)
            {
                var pd = new CScenarioChainingGraph();

                var nodes = Paths.GetCNodes();
                if (nodes is not null && nodes.Length > 0)
                {
                    mb.AddStructureInfo(MetaName.CScenarioChainingNode);
                    pd.Nodes = mb.AddItemArrayPtr(MetaName.CScenarioChainingNode, nodes);
                }
                var edges = Paths.GetCEdges();
                if (edges is not null && edges.Length > 0)
                {
                    mb.AddStructureInfo(MetaName.CScenarioChainingEdge);
                    mb.AddEnumInfo(MetaName.CScenarioChainingEdge__eAction);
                    mb.AddEnumInfo(MetaName.CScenarioChainingEdge__eNavMode);
                    mb.AddEnumInfo(MetaName.CScenarioChainingEdge__eNavSpeed);
                    pd.Edges = mb.AddItemArrayPtr(MetaName.CScenarioChainingEdge, edges);
                }
                if (Paths.Chains is not null)
                {
                    foreach (var chain in Paths.Chains)
                    {
                        if (chain.EdgeIds is not null)
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
                if (chains is not null && chains.Length > 0)
                {
                    mb.AddStructureInfo(MetaName.CScenarioChain);
                    pd.Chains = mb.AddItemArrayPtr(MetaName.CScenarioChain, chains);
                }

                _Data.ChainingGraph = pd;
            }
            else
            {
                _Data.ChainingGraph = new CScenarioChainingGraph();
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

            return mb.AddItemPtr(MetaName.CScenarioPointRegion, in _Data);
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



        public override string Name => Ymt?.ToString() ?? "CScenarioPointRegion";

        public override string ToString() => Name;
    }

    [TC(typeof(EXP))] public struct CScenarioPointContainer  //SCENARIO Region Points arrays // 48 bytes, Key:2489654897
    {
        public Array_Structure LoadSavePoints; //0   0: Array: 0: LoadSavePoints//3016741991  {0: Structure: CExtensionDefSpawnPoint: 256}
        public Array_Structure MyPoints; //16   16: Array: 0: MyPoints//1170781136  {0: Structure: CScenarioPoint//4103049490: 256}
        public uint Unused0 { get; set; }//32
        public uint Unused1 { get; set; }//36
        public uint Unused2 { get; set; }//40
        public uint Unused3 { get; set; }//44

        public override readonly bool Equals(object obj)
        {
            return obj is CScenarioPointContainer container &&
                   EqualityComparer<Array_Structure>.Default.Equals(LoadSavePoints, container.LoadSavePoints) &&
                   EqualityComparer<Array_Structure>.Default.Equals(MyPoints, container.MyPoints) &&
                   Unused0 == container.Unused0 &&
                   Unused1 == container.Unused1 &&
                   Unused2 == container.Unused2 &&
                   Unused3 == container.Unused3;
        }

        public override readonly int GetHashCode()
        {
            int hashCode = 746587205;
            hashCode = hashCode * -1521134295 + LoadSavePoints.GetHashCode();
            hashCode = hashCode * -1521134295 + MyPoints.GetHashCode();
            hashCode = hashCode * -1521134295 + Unused0.GetHashCode();
            hashCode = hashCode * -1521134295 + Unused1.GetHashCode();
            hashCode = hashCode * -1521134295 + Unused2.GetHashCode();
            hashCode = hashCode * -1521134295 + Unused3.GetHashCode();
            return hashCode;
        }

        public override readonly string ToString()
        {
            return $"{LoadSavePoints.Count1} LoadSavePoints, {MyPoints.Count1} MyPoints";
        }

        public static bool operator ==(CScenarioPointContainer left, CScenarioPointContainer right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(CScenarioPointContainer left, CScenarioPointContainer right)
        {
            return !(left == right);
        }
    }
    [TC(typeof(EXP))]
    public class MCScenarioPointContainer : MetaWrapper
    {
        [TC(typeof(EXP))] public object Parent { get; set; }
        public MCScenarioPointRegion Region { get; private set; }

        public CScenarioPointContainer _Data;
        public ref CScenarioPointContainer Data => ref _Data;

        public MCExtensionDefSpawnPoint[] LoadSavePoints { get; set; }
        public MCScenarioPoint[] MyPoints { get; set; }



        public MCScenarioPointContainer() { }
        public MCScenarioPointContainer(MCScenarioPointRegion region)
        {
            Region = region;
        }
        public MCScenarioPointContainer(MCScenarioPointRegion region, Meta meta, in CScenarioPointContainer d)
        {
            Region = region;
            _Data = d;
            Init(meta);
        }

        public void Init(Meta meta)
        {
            var vLoadSavePoints = MetaTypes.ConvertDataArray<CExtensionDefSpawnPoint>(meta, MetaName.CExtensionDefSpawnPoint, in _Data.LoadSavePoints);
            if (vLoadSavePoints != null)
            {
                LoadSavePoints = new MCExtensionDefSpawnPoint[vLoadSavePoints.Length];
                for (int i = 0; i < vLoadSavePoints.Length; i++)
                {
                    LoadSavePoints[i] = new MCExtensionDefSpawnPoint(Region, meta, in vLoadSavePoints[i], this);
                }
            }

            var vMyPoints = MetaTypes.ConvertDataArray<CScenarioPoint>(meta, MetaName.CScenarioPoint, in _Data.MyPoints);
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

        public override void Load(Meta meta, in MetaPOINTER ptr)
        {
            MetaTypes.TryGetData<CScenarioPointContainer>(meta, in ptr, out _Data);
            Init(meta);
        }

        public override MetaPOINTER Save(MetaBuilder mb)
        {
            mb.AddStructureInfo(MetaName.CScenarioPointContainer);
            return mb.AddItemPtr(MetaName.CScenarioPointContainer, in _Data);
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


        public override string Name => "CScenarioPointContainer";
        public override string ToString() => _Data.ToString();
    }

    [TC(typeof(EXP))]
    public struct CScenarioPoint  //SCENARIO Point, similar to CExtensionDefSpawnPointOverride //64 bytes, Key:402442150
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
        public CScenarioPointFlags__Flags Flags { get; set; } //36   36: IntFlags2: 700327466: Flags
        public uint Unused8 { get; set; }//40
        public uint Unused9 { get; set; }//44
        public Vector4 vPositionAndDirection { get; set; } //48   48: Float_XYZW: 0: vPositionAndDirection//4685037

        public override readonly string ToString()
        {
            return FloatUtil.GetVector4String(vPositionAndDirection); //iTimeStartOverride.ToString() + "-" + iTimeEndOverride.ToString();// + ", " + Flags.ToString();
        }
    }
    [TC(typeof(EXP))]
    public class MCScenarioPoint : MetaWrapper
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
                Vector3 dir = value.Multiply(in Vector3.UnitX);
                float dira = (float)Math.Atan2(dir.Y, dir.X);
                Direction = dira;
            }
        }


        public byte TypeId { get { return _Data.iType; } set { _Data.iType = value; } }
        public ScenarioTypeRef Type { get; set; }

        public byte ModelSetId { get { return _Data.ModelSetId; } set { _Data.ModelSetId = value; } }
        public AmbientModelSet ModelSet { get; set; }

        public byte InteriorId {  get { return _Data.iInterior; } set { _Data.iInterior = value; } }
        public MetaHash InteriorName { get; set; }

        public ushort GroupId { get { return _Data.iScenarioGroup; } set { _Data.iScenarioGroup = value; } }
        public MetaHash GroupName { get; set; }

        public byte IMapId { get { return _Data.iRequiredIMapId; } set { _Data.iRequiredIMapId = value; } }
        public MetaHash IMapName { get; set; }

        public string TimeRange => $"{_Data.iTimeStartOverride.ToString().PadLeft(2, '0')}:00 - {_Data.iTimeEndOverride.ToString().PadLeft(2, '0')}:00";
        public byte TimeStart { get { return _Data.iTimeStartOverride; } set { _Data.iTimeStartOverride = value; } }
        public byte TimeEnd { get { return _Data.iTimeEndOverride; } set { _Data.iTimeEndOverride = value; } }
        public byte Probability { get { return _Data.iProbability; } set { _Data.iProbability = value; } }
        public byte AvailableMpSp { get { return _Data.uAvailableInMpSp; } set { _Data.uAvailableInMpSp = value; } }
        public byte Radius { get { return _Data.iRadius; } set { _Data.iRadius = value; } }
        public byte WaitTime { get { return _Data.iTimeTillPedLeaves; } set { _Data.iTimeTillPedLeaves = value; } }
        public CScenarioPointFlags__Flags Flags { get { return _Data.Flags; } set { _Data.Flags = value; } }

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


        public override void Load(Meta meta, in MetaPOINTER ptr)
        {
            MetaTypes.TryGetData<CScenarioPoint>(meta, in ptr, out _Data);
        }

        public override MetaPOINTER Save(MetaBuilder mb)
        {
            mb.AddStructureInfo(MetaName.CScenarioPoint);
            return mb.AddItemPtr(MetaName.CScenarioPoint, in _Data);
        }

        public override string Name => $"{Type?.ToString() ?? string.Empty} : {ModelSet?.ToString() ?? string.Empty}";
        public override string ToString() => $"{Name}: {TimeRange}";

    }

    [TC(typeof(EXP))]
    public struct CScenarioEntityOverride  //SCENARIO Entity Override //80 bytes, Key:1271200492
    {
        public Vector3 EntityPosition; //0   0: Float_XYZ: 0: EntityPosition//642078041
        public float Unused00 { get; set; }//12
        public MetaHash EntityType { get; set; } //16   16: Hash: 0: EntityType//1374199246
        public uint Unused01 { get; set; }//20
        public Array_Structure ScenarioPoints; //24   24: Array: 0: ScenarioPoints  {0: Structure: CExtensionDefSpawnPoint: 256}
        public uint Unused02 { get; set; }//40
        public uint Unused03 { get; set; }//44
        public uint Unused04 { get; set; }//48
        public uint Unused05 { get; set; }//52
        public uint Unused06 { get; set; }//56
        public uint Unused07 { get; set; }//60
        public byte EntityMayNotAlwaysExist { get; set; } //64   64: Boolean: 0: EntityMayNotAlwaysExist
        public byte SpecificallyPreventArtPoints { get; set; } //65   65: Boolean: 0: SpecificallyPreventArtPoints
        public ushort Unused08 { get; set; }//66
        public uint Unused09 { get; set; }//68
        public uint Unused10 { get; set; }//72
        public uint Unused11 { get; set; }//76

        public override readonly string ToString() => $"{EntityType}, {ScenarioPoints.Count1} ScenarioPoints";
    }

    [TC(typeof(EXP))]
    public class MCScenarioEntityOverride : MetaWrapper
    {
        [TC(typeof(EXP))] public object Parent { get; set; }
        public MCScenarioPointRegion Region { get; set; }

        public CScenarioEntityOverride _Data;
        public CScenarioEntityOverride Data { get { return _Data; } set { _Data = value; } }

        public Vector3 Position { get { return _Data.EntityPosition; } set { _Data.EntityPosition = value; } }

        public MetaHash TypeName {  get { return _Data.EntityType; } set { _Data.EntityType = value; } }
        public bool EntityMayNotAlwaysExist { get { return _Data.EntityMayNotAlwaysExist == 1; } set { _Data.EntityMayNotAlwaysExist = (byte)(value ? 1 : 0); } }
        public bool SpecificallyPreventArtPoints { get { return _Data.SpecificallyPreventArtPoints == 1; } set { _Data.SpecificallyPreventArtPoints = (byte)(value ? 1 : 0); } }


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
        public MCScenarioEntityOverride(MCScenarioPointRegion region, Meta meta, in CScenarioEntityOverride d)
        {
            Region = region;
            _Data = d;
            Init(meta);
        }

        public void Init(Meta meta)
        {

            var scenarioPoints = MetaTypes.ConvertDataArray<CExtensionDefSpawnPoint>(meta, MetaName.CExtensionDefSpawnPoint, in _Data.ScenarioPoints);
            if (scenarioPoints != null)
            {
                ScenarioPoints = new MCExtensionDefSpawnPoint[scenarioPoints.Length];
                for (int i = 0; i < scenarioPoints.Length; i++)
                {
                    ScenarioPoints[i] = new MCExtensionDefSpawnPoint(Region, meta, in scenarioPoints[i], this);
                    ScenarioPoints[i].ParentPosition = Position;
                }
            }

        }

        public override void Load(Meta meta, in MetaPOINTER ptr)
        {
            MetaTypes.TryGetData<CScenarioEntityOverride>(meta, in ptr, out _Data);
            Init(meta);
        }

        public override MetaPOINTER Save(MetaBuilder mb)
        {
            mb.AddStructureInfo(MetaName.CScenarioEntityOverride);

            if (ScenarioPoints != null)
            {
                mb.AddStructureInfo(MetaName.CExtensionDefSpawnPoint);
                mb.AddEnumInfo(MetaName.CSpawnPoint__AvailabilityMpSp);
                mb.AddEnumInfo(MetaName.CScenarioPointFlags__Flags);
                _Data.ScenarioPoints = mb.AddWrapperArray(ScenarioPoints);
            }

            return mb.AddItemPtr(MetaName.CScenarioEntityOverride, in _Data);
        }


        public void AddScenarioPoint(MCExtensionDefSpawnPoint p)
        {
            List<MCExtensionDefSpawnPoint> newpoints = new List<MCExtensionDefSpawnPoint>();
            if (ScenarioPoints is not null)
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



        public override string Name => $"CScenarioEntityOverride {_Data.EntityType}";
        public override string ToString() => _Data.ToString();
    }

    [TC(typeof(EXP))]
    public record struct CScenarioChainingGraph  //SCENARIO PATH ARRAYS //88 bytes, Key:88255871
    {
        public Array_Structure Nodes; //0   0: Array: 0: Nodes  {0: Structure: CScenarioChainingNode//3340683255: 256}
        public Array_Structure Edges; //16   16: Array: 0: Edges  {0: Structure: CScenarioChainingEdge//4255409560: 256}
        public Array_Structure Chains; //32   32: Array: 0: Chains  {0: Structure: CScenarioChain: 256}
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

        public override readonly string ToString()
        {
            return $"{Nodes.Count1} Nodes, {Edges.Count1} Edges, {Chains.Count1} Chains";
        }
    }

    [TC(typeof(EXP))]
    public class MCScenarioChainingGraph : MetaWrapper
    {
        public MCScenarioPointRegion Region { get; private set; }

        public CScenarioChainingGraph _Data;
        public CScenarioChainingGraph Data { get { return _Data; } set { _Data = value; } }

        public MCScenarioChainingNode[] Nodes { get; set; }
        public MCScenarioChainingEdge[] Edges { get; set; }
        public MCScenarioChain[] Chains { get; set; }

        public MCScenarioChainingGraph() { }
        public MCScenarioChainingGraph(MCScenarioPointRegion region) { Region = region; }
        public MCScenarioChainingGraph(MCScenarioPointRegion region, Meta meta, CScenarioChainingGraph d)
        {
            Region = region;
            _Data = d;
            Init(meta);
        }

        public void Init(Meta meta)
        {
            var pathnodes = MetaTypes.ConvertDataArray<CScenarioChainingNode>(meta, MetaName.CScenarioChainingNode, in _Data.Nodes);
            if (pathnodes != null)
            {
                Nodes = new MCScenarioChainingNode[pathnodes.Length];
                for (int i = 0; i < pathnodes.Length; i++)
                {
                    Nodes[i] = new MCScenarioChainingNode(Region, meta, pathnodes[i], this, i);
                }
            }

            var pathedges = MetaTypes.ConvertDataArray<CScenarioChainingEdge>(meta, MetaName.CScenarioChainingEdge, in _Data.Edges);
            if (pathedges != null)
            {
                Edges = new MCScenarioChainingEdge[pathedges.Length];
                for (int i = 0; i < pathedges.Length; i++)
                {
                    Edges[i] = new MCScenarioChainingEdge(Region, meta, pathedges[i], i);
                }
            }

            var pathchains = MetaTypes.ConvertDataArray<CScenarioChain>(meta, MetaName.CScenarioChain, in _Data.Chains);
            if (pathchains != null)
            {
                Chains = new MCScenarioChain[pathchains.Length];
                for (int i = 0; i < pathchains.Length; i++)
                {
                    Chains[i] = new MCScenarioChain(Region, meta, pathchains[i]);
                }
            }
        }

        public override void Load(Meta meta, in MetaPOINTER ptr)
        {
            MetaTypes.TryGetData<CScenarioChainingGraph>(meta, in ptr, out _Data);
            Init(meta);
        }



        public override MetaPOINTER Save(MetaBuilder mb)
        {
            mb.AddStructureInfo(MetaName.CScenarioChainingGraph);
            return mb.AddItemPtr(MetaName.CScenarioChainingGraph, in _Data);
        }







        public CScenarioChainingNode[] GetCNodes()
        {
            if (Nodes == null || Nodes.Length == 0)
                return null;
            CScenarioChainingNode[] r = new CScenarioChainingNode[Nodes.Length];
            for (int i = 0; i < Nodes.Length; i++)
            {
                r[i] = Nodes[i]._Data;
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

                    var remchains = new List<MCScenarioChain>();
                    foreach (var c in Chains)
                    {
                        if (c == null) continue;
                        if (c.RemoveEdge(e))
                        {
                            if ((c.Edges?.Length ?? 0) == 0)
                            {
                                remchains.Add(c);
                            }
                        }
                    }
                    foreach (var c in remchains)
                    {
                        RemoveChain(c);
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
                return "CScenarioChainingGraph " + _Data.ToString();
            }
        }
        public override string ToString()
        {
            return _Data.ToString();
        }
    }

    [TC(typeof(EXP))]
    public struct CScenarioChainingNode //SCENARIO PATH NODE //32 bytes, Key:1811784424
    {
        public Vector3 Position { get; set; } //0   0: Float_XYZ: 0: Position
        public float Unused0 { get; set; }//12
        public MetaHash Unk_2602393771 { get; set; } //16   16: Hash: 0: 2602393771  prop name, eg. prop_parknmeter_01, prop_atm_01, prop_bench_01a
        public MetaHash ScenarioType { get; set; } //20   20: Hash: 0: ScenarioType
        public byte HasIncomingEdges { get; set; } //24   24: Boolean: 0: HasIncomingEdges (not first node)
        public byte HasOutgoingEdges { get; set; } //25   25: Boolean: 0: HasOutgoingEdges (not last node)
        public ushort Unused1 { get; set; }//26
        public uint Unused2 { get; set; }//28

        public override string ToString()
        {
            return $"{ScenarioType}, {Unk_2602393771}";
        }
    }
    [TC(typeof(EXP))] public class MCScenarioChainingNode : MetaWrapper
    {
        [TC(typeof(EXP))] public MCScenarioChainingGraph Parent { get; set; }
        public MCScenarioPointRegion Region { get; set; }
        public ScenarioNode ScenarioNode { get; set; }

        public CScenarioChainingNode _Data;
        public CScenarioChainingNode Data { get { return _Data; } set { _Data = value; } }

        public Vector3 Position { get { return _Data.Position; } set { _Data.Position = value; } }
        public MetaHash PropHash { get { return _Data.Unk_2602393771; } set { _Data.Unk_2602393771 = value; } }
        public MetaHash TypeHash { get { return _Data.ScenarioType; } set { _Data.ScenarioType = value; } }
        public ScenarioTypeRef Type { get; set; }
        public bool HasIncomingEdges { get { return _Data.HasIncomingEdges == 1; } set { _Data.HasIncomingEdges = (byte)(value ? 1 : 0); } }
        public bool HasOutgoingEdges { get { return _Data.HasOutgoingEdges == 1; } set { _Data.HasOutgoingEdges = (byte)(value ? 1 : 0); } }

        public int NodeIndex { get; set; }
        public MCScenarioChain Chain { get; set; }


        public MCScenarioChainingNode() { }
        public MCScenarioChainingNode(MCScenarioPointRegion region, Meta meta, CScenarioChainingNode d, MCScenarioChainingGraph parent, int index)
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

        public override void Load(Meta meta, in MetaPOINTER ptr)
        {
            MetaTypes.TryGetData<CScenarioChainingNode>(meta, in ptr, out _Data);
        }

        public override MetaPOINTER Save(MetaBuilder mb)
        {
            mb.AddStructureInfo(MetaName.CScenarioChainingNode);
            return mb.AddItemPtr(MetaName.CScenarioChainingNode, in _Data);
        }

        public override string Name => "CScenarioChainingNode";


        public override string ToString() => _Data.ToString();

    }

    [TC(typeof(EXP))] public struct CScenarioChainingEdge  //SCENARIO PATH EDGE //8 bytes, Key:2004985940
    {
        public ushort NodeIndexFrom { get; set; } //0   0: UnsignedShort: 0: NodeIndexFrom//3236798246
        public ushort NodeIndexTo { get; set; } //2   2: UnsignedShort: 0: NodeIndexTo//2851806039
        public CScenarioChainingEdge__eAction Action { get; set; } //4   4: ByteEnum: CScenarioChainingEdge__eAction: Action
        public CScenarioChainingEdge__eNavMode NavMode { get; set; } //5   5: ByteEnum: CScenarioChainingEdge__eNavMode: NavMode
        public CScenarioChainingEdge__eNavSpeed NavSpeed { get; set; } //6   6: ByteEnum: CScenarioChainingEdge__eNavSpeed: NavSpeed
        public byte Unused0 { get; set; }//7

        public override string ToString()
        {
            return $"{NodeIndexFrom}, {NodeIndexTo}, {Action}, {NavMode}, {NavSpeed}";
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
        public CScenarioChainingEdge__eAction Action { get { return _Data.Action; } set { _Data.Action = value; } }
        public CScenarioChainingEdge__eNavMode NavMode { get { return _Data.NavMode; } set { _Data.NavMode = value; } }
        public CScenarioChainingEdge__eNavSpeed NavSpeed { get { return _Data.NavSpeed; } set { _Data.NavSpeed = value; } }


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

        public override void Load(Meta meta, in MetaPOINTER ptr)
        {
            MetaTypes.TryGetData<CScenarioChainingEdge>(meta, in ptr, out _Data);
        }

        public override MetaPOINTER Save(MetaBuilder mb)
        {
            mb.AddStructureInfo(MetaName.CScenarioChainingEdge);
            return mb.AddItemPtr(MetaName.CScenarioChainingEdge, in _Data);
        }

        public override string Name => "CScenarioChainingEdge";

        public override string ToString()
        {
            return $"{Action}, {NavMode}, {NavSpeed}";
        }

    }

    [TC(typeof(EXP))] public struct CScenarioChain  //SCENARIO PATH CHAIN //40 bytes, Key:2751910366
    {
        public byte Unk_1156691834 { get; set; } //0   0: UnsignedByte: 0: 1156691834
        public byte Unused0 { get; set; }//1
        public ushort Unused1 { get; set; }//2
        public uint Unused2 { get; set; }//4
        public Array_ushort EdgeIds; //8   8: Array: 0: EdgeIds  {0: UnsignedShort: 0: 256}
        public uint Unused3 { get; set; }//24
        public uint Unused4 { get; set; }//28
        public uint Unused5 { get; set; }//32
        public uint Unused6 { get; set; }//36

        public override readonly string ToString()
        {
            return $"{Unk_1156691834}: {EdgeIds.Count1} EdgeIds";
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
            EdgeIds = MetaTypes.GetUshortArray(meta, in _Data.EdgeIds);
        }

        public override void Load(Meta meta, in MetaPOINTER ptr)
        {
            MetaTypes.TryGetData<CScenarioChain>(meta, in ptr, out _Data);
            EdgeIds = MetaTypes.GetUshortArray(meta, in _Data.EdgeIds);
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
            //    mb.AddStructureInfo((MetaName)MetaTypeName.USHORT);
            //    _Data.EdgeIds = mb.AddItemArrayPtr((MetaName)MetaTypeName.USHORT, EdgeIds);
            //}

            mb.AddStructureInfo(MetaName.CScenarioChain);
            return mb.AddItemPtr(MetaName.CScenarioChain, in _Data);
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
        public CScenarioPointContainer Points; //0   0: Structure: CScenarioPointContainer//2380938603: Points//702683191
        public rage__spdSphere ClusterSphere; //48   48: Structure: 1062159465: ClusterSphere//352461053
        public float Unk_1095875445 { get; set; } //64   64: Float: 0: 1095875445 //spawn chance? eg 5, 30
        public byte Unk_3129415068 { get; set; } //68   68: Boolean: 0: 3129415068
        public uint Unused0 { get; set; }//72
        public uint Unused1 { get; set; }//76

        public override readonly string ToString()
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
        public MCScenarioPointCluster(MCScenarioPointRegion region, Meta meta, in CScenarioPointCluster d)
        {
            Region = region;
            _Data = d;
            Points = new MCScenarioPointContainer(region, meta, in d.Points);
            Points.Parent = this;
        }

        public override void Load(Meta meta, in MetaPOINTER ptr)
        {
            MetaTypes.TryGetData<CScenarioPointCluster>(meta, in ptr, out _Data);
            Points = new MCScenarioPointContainer(Region, meta, in _Data.Points);
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
                    mb.AddEnumInfo(MetaName.CSpawnPoint__AvailabilityMpSp);
                    mb.AddEnumInfo(MetaName.CScenarioPointFlags__Flags);
                    scp.LoadSavePoints = mb.AddItemArrayPtr(MetaName.CExtensionDefSpawnPoint, loadSavePoints);
                }
                var myPoints = Points.GetCMyPoints();
                if (myPoints != null)
                {
                    mb.AddStructureInfo(MetaName.CScenarioPoint);
                    mb.AddEnumInfo(MetaName.CScenarioPointFlags__Flags);
                    scp.MyPoints = mb.AddItemArrayPtr(MetaName.CScenarioPoint, myPoints);
                }

                _Data.Points = scp;
            }
            else
            {
                _Data.Points = new CScenarioPointContainer();
            }

            return mb.AddItemPtr(MetaName.CScenarioPointCluster, in _Data);
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

        public override readonly string ToString()
        {
            return "CScenarioPointLookUps";
        }

        public override readonly bool Equals(object obj)
        {
            return obj is CScenarioPointLookUps ups &&
                   EqualityComparer<Array_uint>.Default.Equals(TypeNames, ups.TypeNames) &&
                   EqualityComparer<Array_uint>.Default.Equals(PedModelSetNames, ups.PedModelSetNames) &&
                   EqualityComparer<Array_uint>.Default.Equals(VehicleModelSetNames, ups.VehicleModelSetNames) &&
                   EqualityComparer<Array_uint>.Default.Equals(GroupNames, ups.GroupNames) &&
                   EqualityComparer<Array_uint>.Default.Equals(InteriorNames, ups.InteriorNames) &&
                   EqualityComparer<Array_uint>.Default.Equals(RequiredIMapNames, ups.RequiredIMapNames);
        }

        public override readonly int GetHashCode()
        {
            int hashCode = -153113894;
            hashCode = hashCode * -1521134295 + TypeNames.GetHashCode();
            hashCode = hashCode * -1521134295 + PedModelSetNames.GetHashCode();
            hashCode = hashCode * -1521134295 + VehicleModelSetNames.GetHashCode();
            hashCode = hashCode * -1521134295 + GroupNames.GetHashCode();
            hashCode = hashCode * -1521134295 + InteriorNames.GetHashCode();
            hashCode = hashCode * -1521134295 + RequiredIMapNames.GetHashCode();
            return hashCode;
        }

        public static bool operator ==(CScenarioPointLookUps left, CScenarioPointLookUps right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(CScenarioPointLookUps left, CScenarioPointLookUps right)
        {
            return !(left == right);
        }
    }
    [TC(typeof(EXP))] public class MCScenarioPointLookUps : MetaWrapper
    {
        public MCScenarioPointRegion Region { get; set; }

        public CScenarioPointLookUps _Data;
        public CScenarioPointLookUps Data => _Data;

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
        public MCScenarioPointLookUps(MCScenarioPointRegion region, Meta meta, in CScenarioPointLookUps d)
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

        public override void Load(Meta meta, in MetaPOINTER ptr)
        {
            MetaTypes.TryGetData<CScenarioPointLookUps>(meta, in ptr, out _Data);
            Init(meta);
        }

        public override MetaPOINTER Save(MetaBuilder mb)
        {
            mb.AddStructureInfo(MetaName.CScenarioPointLookUps);
            return mb.AddItemPtr(MetaName.CScenarioPointLookUps, in _Data);
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

    [TC(typeof(EXP))]
    public readonly struct CCompositeEntityType //304 bytes, Key:659539004 dex: composite entity type - ytyp // Tom: des_ destruction
    {
        public ArrayOfChars64 Name { get; init; } //0   0: ArrayOfChars: 64: Name
        public float lodDist { get; init; } //64   64: Float: 0: lodDist
        public uint flags { get; init; } //68   68: UnsignedInt: 0: flags
        public uint specialAttribute { get; init; } //72   72: UnsignedInt: 0: specialAttribute
        public uint Unused0 { get; init; }//76
        public Vector3 bbMin { get; init; } //80   80: Float_XYZ: 0: bbMin
        public float Unused1 { get; init; }//92
        public Vector3 bbMax { get; init; } //96   96: Float_XYZ: 0: bbMax
        public float Unused2 { get; init; }//108
        public Vector3 bsCentre { get; init; } //112   112: Float_XYZ: 0: bsCentre
        public float Unused3 { get; init; }//124
        public float bsRadius { get; init; } //128   128: Float: 0: bsRadius
        public uint Unused4 { get; init; }//132
        public ArrayOfChars64 StartModel { get; init; } //136   136: ArrayOfChars: 64: StartModel
        public ArrayOfChars64 EndModel { get; init; } //200   200: ArrayOfChars: 64: EndModel
        public MetaHash StartImapFile { get; init; } //264   264: Hash: 0: StartImapFile//2462971690
        public MetaHash EndImapFile { get; init; } //268   268: Hash: 0: EndImapFile//2059586669
        public MetaHash PtFxAssetName { get; init; } //272   272: Hash: 0: PtFxAssetName//2497993358
        public uint Unused5 { get; init; }//276
        public Array_Structure Animations { get; init; } //280   280: Array: 0: Animations  {0: Structure: 1980345114: 256}
        public uint Unused6 { get; init; }//296
        public uint Unused7 { get; init; }//300

        public override string ToString()
        {
            return $"{Name}, {StartModel}, {EndModel}, {StartImapFile}, {EndImapFile}, {PtFxAssetName}";
        }
    }

    [TC(typeof(EXP))]
    public readonly struct CCompEntityAnims //216 bytes, Key:4110496011 //destruction animations?
    {
        public ArrayOfChars64 AnimDict { get; init; } //0   0: ArrayOfChars: 64: AnimDict
        public ArrayOfChars64 AnimName { get; init; } //64   64: ArrayOfChars: 64: AnimName
        public ArrayOfChars64 AnimatedModel { get; init; } //128   128: ArrayOfChars: 64: AnimatedModel
        public float punchInPhase { get; init; } //192   192: Float: 0: punchInPhase//3142377407
        public float punchOutPhase { get; init; } //196   196: Float: 0: punchOutPhase//2164219370
        public Array_Structure effectsData { get; init; } //200   200: Array: 0: effectsData  {0: Structure: 3430328684: 256}
    }

    [TC(typeof(EXP))]
    public struct CCompEntityEffectsData //160 bytes, Key:1724963966 //destruction animation effects data
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

    [TC(typeof(EXP))]
    public struct CStreamingRequestRecord //40 bytes, Key:3825587854  //SRL YMT ROOT - in /streaming/ folder
    {
        public Array_Structure Frames { get; set; } //0   0: Array: 0: Frames  {0: Structure: CStreamingRequestFrame: 256}
        public Array_Structure CommonSets { get; set; } //16   16: Array: 0: CommonSets  {0: Structure: CStreamingRequestCommonSet: 256}
        public byte NewStyle { get; set; } //32   32: Boolean: 0: NewStyle
        public byte Unused0 { get; set; }//33
        public ushort Unused1 { get; set; }//34
        public uint Unused2 { get; set; }//36
    }

    [TC(typeof(EXP))]
    public struct CStreamingRequestFrame //112 bytes, Key:1112444512  //SRL frame...
    {
        public Array_uint AddList { get; set; } //0   0: Array: 0: AddList//327274266  {0: Hash: 0: 256}
        public Array_uint RemoveList { get; set; } //16   16: Array: 0: RemoveList//3372321331  {0: Hash: 0: 256}
        public Array_uint PromoteToHDList { get; set; } //32   32: Array: 0: 896120921  {0: Hash: 0: 256}
        public Vector3 CamPos { get; set; } //48   48: Float_XYZ: 0: CamPos//357008256
        public float Unused0 { get; set; }//60
        public Vector3 CamDir { get; set; } //64   64: Float_XYZ: 0: CamDir//210316193
        public float Unused1 { get; set; }//76
        public Array_byte CommonAddSets { get; set; } //80   80: Array: 0: 1762439591  {0: UnsignedByte: 0: 256}
        public uint Flags { get; set; } //96   96: UnsignedInt: 0: Flags
        public uint Unused2 { get; set; }//100
        public uint Unused3 { get; set; }//104
        public uint Unused4 { get; set; }//108
    }

    [TC(typeof(EXP))]
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

    [TC(typeof(EXP))] public struct CStreamingRequestCommonSet //16 bytes, Key:3710200606   //SRL common set
    {
        public Array_uint Requests { get; set; } //0   0: Array: 0: Requests  {0: Hash: 0: 256}
    }














    [TC(typeof(EXP))] public struct CCreatureMetaData //56 bytes, Key:2181653572
    {
        public uint Unused0 { get; set; }//0
        public uint Unused1 { get; set; }//4
        public Array_Structure shaderVariableComponents { get; set; } //8   8: Array: 0: shaderVariableComponents  {0: Structure: CShaderVariableComponent: 256}
        public Array_Structure pedPropExpressions { get; set; } //24   24: Array: 0: pedPropExpressions  {0: Structure: CPedPropExpressionData: 256}
        public Array_Structure pedCompExpressions { get; set; } //40   40: Array: 0: pedCompExpressions  {0: Structure: CPedCompExpressionData: 256}
    }

    [TC(typeof(EXP))] public struct CShaderVariableComponent //72 bytes, Key:3085831725
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

    [TC(typeof(EXP))] public struct CPedPropExpressionData //88 bytes, Key:1355135810
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

    [TC(typeof(EXP))] public struct CPedCompExpressionData //88 bytes, Key:3458164745
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




    [TC(typeof(EXP))] public struct CPedVariationInfo : IPsoSwapEnd<CPedVariationInfo> //112 bytes, Key:4030871161  //COMPONENT PEDS YMT ROOT  - in componentpeds .rpf's
    {
        public byte bHasTexVariations { get; set; } //0   0: Boolean: 0: bHasTexVariations
        public byte bHasDrawblVariations { get; set; } //1   1: Boolean: 0: bHasDrawblVariations
        public byte bHasLowLODs { get; set; } //2   2: Boolean: 0: bHasLowLODs
        public byte bIsSuperLOD { get; set; } //3   3: Boolean: 0: bIsSuperLOD
        public ArrayOfBytes12 availComp; //4   4: ArrayOfBytes: 12: availComp
        public Array_Structure aComponentData3; //16   16: Array: 0: aComponentData3  {0: Structure: CPVComponentData: 256}
        public Array_Structure aSelectionSets; //32   32: Array: 0: aSelectionSets  {0: Structure: CPedSelectionSet: 256}
        public Array_Structure compInfos; //48   48: Array: 0: compInfos  {0: Structure: CComponentInfo: 256}
        public CPedPropInfo propInfo; //64   64: Structure: CPedPropInfo: propInfo
        public MetaHash dlcName { get; set; } //104   104: Hash: 0: dlcName
        public uint Unused0 { get; set; }//108

        public CPedVariationInfo SwapEnd()
        {
            return this with
            {
                aComponentData3 = aComponentData3.SwapEnd(),
                aSelectionSets = aSelectionSets.SwapEnd(),
                compInfos = compInfos.SwapEnd(),
                propInfo = propInfo.SwapEnd(),
                dlcName = MetaTypes.SwapBytes(dlcName),
            };
        }
    }
    [TC(typeof(EXP))] public class MCPedVariationInfo : MetaWrapper
    {
        public CPedVariationInfo _Data;
        public CPedVariationInfo Data => _Data;

        public byte[] ComponentIndices { get; set; }
        public MCPVComponentData[] ComponentData3 { get; set; }
        public MCPedSelectionSet[] SelectionSets { get; set; }
        public MCComponentInfo[] CompInfos { get; set; }
        public MCPedPropInfo PropInfo { get; set; }


        public override void Load(Meta meta, in MetaPOINTER ptr)
        {
            MetaTypes.TryGetData<CPedVariationInfo>(meta, in ptr, out var data);
            Load(meta, in data);
        }
        public void Load(Meta meta, in CPedVariationInfo data)
        {
            //maybe see https://github.com/emcifuntik/altv-cloth-tool/blob/master/AltTool/ResourceBuilder.cs


            _Data = data;

            ComponentIndices = data.availComp.GetArray();


            var aComponentData3 = MetaTypes.ConvertDataArray<CPVComponentData>(meta, MetaName.CPVComponentData, in _Data.aComponentData3);
            if (aComponentData3 != null)
            {
                ComponentData3 = new MCPVComponentData[aComponentData3.Length];
                for (int i = 0; i < aComponentData3.Length; i++)
                {
                    ComponentData3[i] = new MCPVComponentData(meta, in aComponentData3[i], this);
                }
            }

            var vSelectionSets = MetaTypes.ConvertDataArray<CPedSelectionSet>(meta, MetaName.CPedSelectionSet, in _Data.aSelectionSets);
            if (vSelectionSets != null)
            {
                SelectionSets = new MCPedSelectionSet[vSelectionSets.Length];
                for (int i = 0; i < vSelectionSets.Length; i++)
                {
                    SelectionSets[i] = new MCPedSelectionSet(meta, vSelectionSets[i], this);
                }
            }

            var vCompInfos = MetaTypes.ConvertDataArray<CComponentInfo>(meta, MetaName.CComponentInfo, in _Data.compInfos);
            if (vCompInfos != null)
            {
                CompInfos = new MCComponentInfo[vCompInfos.Length];
                for (int i = 0; i < vCompInfos.Length; i++)
                {
                    CompInfos[i] = new MCComponentInfo(meta, in vCompInfos[i], this);
                }
            }

            PropInfo = new MCPedPropInfo(meta, in data.propInfo, this);



            for (int i = 0; i < 12; i++) //set the component type indices on all the component variants, for them to use
            {
                var compInd = ComponentIndices[i];
                if ((compInd >= 0) && (compInd < ComponentData3?.Length))
                {
                    var compvar = ComponentData3[compInd];
                    compvar.ComponentType = i;

                    if (compvar.DrawblData3 != null)
                    {
                        foreach (var cvp in compvar.DrawblData3)
                        {
                            cvp.ComponentType = i;
                            //cvp.GetDrawableName();//testing
                        }
                    }
                }
            }


            foreach(var component in ComponentData3)
            {
                if (component.ComponentType == -1)
                {
                    Console.WriteLine($"No component type found for {component.Name} {component.Owner.Name}");
                }
            }

        }
        public void Load(PsoFile pso, in CPedVariationInfo data)
        {
            //TODO!
        }

        public override MetaPOINTER Save(MetaBuilder mb)
        {
            throw new NotImplementedException();
        }


        public MCPVComponentData? GetComponentData(int componentType)
        {
            if ((componentType < 0) || (componentType > 11))
                throw new ArgumentOutOfRangeException(nameof(componentType), componentType, "Value should fall in range 0-11");
            if (ComponentIndices is null || ComponentData3 is null)
                return null;

            var index = ComponentIndices[componentType];
            // Apparantly some files (like mp_f_stunt_01) have more datas than indices for some reason?
            // I don't know if this is a mistake made by them or that this data is used for something else, but for now just discard it
            if (index > ComponentData3?.Length || ComponentData3[index].ComponentType == -1)
                return null;
            return ComponentData3[index];
        }

    }

    [TC(typeof(EXP))]
    public readonly struct CPVComponentData //24 bytes, Key:2024084511  //COMPONENT PEDS component variations item
    {
        public byte numAvailTex { get; init; } //0   0: UnsignedByte: 0: numAvailTex
        public byte Unused0 { get; init; }//1
        public ushort Unused1 { get; init; }//2
        public uint Unused2 { get; init; }//4
        public readonly Array_Structure aDrawblData3; //8   8: Array: 0: aDrawblData3  {0: Structure: CPVDrawblData: 256}
    }
    [TC(typeof(EXP))] public class MCPVComponentData : MetaWrapper
    {
        public MCPedVariationInfo Owner { get; set; }

        public CPVComponentData _Data;
        public CPVComponentData Data => _Data;

        public byte numAvailTex => _Data.numAvailTex;

        public MCPVDrawblData[] DrawblData3 { get; set; }

        public int ComponentType { get; set; } = -1;
        public static string[] ComponentTypeNames { get; } =
        {
            "head",//0
            "berd",//1
            "hair",//2
            "uppr",//3
            "lowr",//4
            "hand",//5
            "feet",//6
            "teef",//7
            "accs",//8
            "task",//9
            "decl",//10
            "jbib",//11
        };


        public MCPVComponentData() { }
        public MCPVComponentData(Meta meta, in CPVComponentData data, MCPedVariationInfo owner)
        {
            _Data = data;
            Owner = owner;
            Init(meta);
        }


        private void Init(Meta meta)
        {
            var aDrawblData3 = MetaTypes.ConvertDataArray<CPVDrawblData>(meta, MetaName.CPVDrawblData, in _Data.aDrawblData3);
            if (aDrawblData3 != null)
            {
                DrawblData3 = new MCPVDrawblData[aDrawblData3.Length];
                for (int i = 0; i < aDrawblData3.Length; i++)
                {
                    DrawblData3[i] = new MCPVDrawblData(meta, in aDrawblData3[i], this, i);
                }
            }
        }


        public override void Load(Meta meta, in MetaPOINTER ptr)
        {
            MetaTypes.TryGetData<CPVComponentData>(meta, in ptr, out _Data);
            Init(meta);
        }

        public override MetaPOINTER Save(MetaBuilder mb)
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            string r = (ComponentType < 12 && ComponentType >= 0) ? ComponentTypeNames[ComponentType] : "error";
            return r + " : " + (DrawblData3?.Length.ToString() ?? base.ToString());
        }
    }

    [TC(typeof(EXP))]
    public readonly struct CPVDrawblData //48 bytes, Key:124073662  //COMPONENT PEDS drawable info
    {
        public byte propMask { get; init; } //0   0: UnsignedByte: 0: propMask
        public byte numAlternatives { get; init; } //1   1: UnsignedByte: 0: 2806194106
        public ushort Unused0 { get; init; }//2
        public uint Unused1 { get; init; }//4
        public readonly Array_Structure aTexData; //8   8: Array: 0: aTexData  {0: Structure: CPVTextureData: 256}
        public readonly CPVDrawblData__CPVClothComponentData clothData; //24   24: Structure: CPVDrawblData__CPVClothComponentData: clothData
    }
    [TC(typeof(EXP))] public class MCPVDrawblData : MetaWrapper
    {
        public MCPVComponentData Owner { get; set; }

        public CPVDrawblData _Data;
        public CPVDrawblData Data => _Data;

        public CPVTextureData[] TexData { get; set; }

        public int ComponentType { get; set; } = 0;
        public int DrawableIndex { get; set; } = 0;
        public int PropMask => _Data.propMask;
        public int NumAlternatives => _Data.numAlternatives;

        public int PropType => (PropMask >> 4) & 3;

        public string GetDrawableName(int altnum = 0)
        {
            string r = (ComponentType < 12) ? MCPVComponentData.ComponentTypeNames[ComponentType] : "error";
            r += "_";
            r += DrawableIndex.ToString("000");
            r += "_";
            switch (PropType)
            {
                case 0: r += "u"; break;//what do these mean?
                case 1: r += "r"; break;
                case 2: r += "m"; break;
                case 3: r += "m"; break;
                default:
                    break;
            }
            if (altnum > 0)
            {
                r += "_";
                r += altnum.ToString();
            }
            return r;
        }

        public string GetTextureName(int texnum = 0)
        {
            return GetTexturePrefix() + GetTextureSuffix(texnum);
        }
        public string GetTexturePrefix()
        {
            string r = (ComponentType < 12) ? MCPVComponentData.ComponentTypeNames[ComponentType] : "error";
            r += "_diff_"; //are there variations of this?
            r += DrawableIndex.ToString("000");
            r += "_";
            return r;
        }
        public string GetTextureSuffix(int texnum)
        {
            if (texnum < 0) texnum = 0;
            const string alphas = "abcdefghijklmnopqrstuvwxyz";
            var tex = TexData[texnum];
            var texid = tex.texId;
            var distr = tex.distribution;//what does this do?
            var r = string.Empty;
            r += alphas[texnum % 26];
            r += "_";
            switch (texid)
            {
                case 0:
                    r += "uni";
                    break;
                case 1:
                    r += "whi";
                    break;
                case 2:
                    r += "bla";
                    break;
                case 3:
                    r += "chi";
                    break;
                case 4:
                    r += "lat";
                    break;
                case 5:
                    r += "ara";
                    break;
                case 8:
                    r += "kor";
                    break;
                case 10:
                    r += "pak";
                    break;
                default:
                    r += "whi";
                    break;
            }
            return r;
        }


        public MCPVDrawblData() { }
        public MCPVDrawblData(Meta meta, in CPVDrawblData data, MCPVComponentData owner, int index)
        {
            _Data = data;
            Owner = owner;
            DrawableIndex = index;

            TexData = MetaTypes.ConvertDataArray<CPVTextureData>(meta, MetaName.CPVTextureData, in _Data.aTexData);
        }


        public override void Load(Meta meta, in MetaPOINTER ptr)
        {
            throw new NotImplementedException();
        }

        public override MetaPOINTER Save(MetaBuilder mb)
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return GetDrawableName();
        }
    }

    [TC(typeof(EXP))]
    public struct CPVTextureData //3 bytes, Key:4272717794  //COMPONENT PEDS (cloth?) aTexData
    {
        public byte texId { get; set; } //0   0: UnsignedByte: 0: texId
        public byte distribution { get; set; } //1   1: UnsignedByte: 0: distribution//914976023
        public byte Unused0 { get; set; }//2
    }

    [TC(typeof(EXP))]
    public readonly struct CPVDrawblData__CPVClothComponentData //24 bytes, Key:508935687  //COMPONENT PEDS clothData
    {
        public byte ownsCloth { get; init; } //0   0: Boolean: 0: ownsCloth
        public byte Unused0 { get; init; }//1
        public ushort Unused1 { get; init; }//2
        public uint Unused2 { get; init; }//4
        public uint Unused3 { get; init; }//8
        public uint Unused4 { get; init; }//12
        public uint Unused5 { get; init; }//16
        public uint Unused6 { get; init; }//20
    }

    [TC(typeof(EXP))]
    public readonly struct CPedSelectionSet //48 bytes, Key:3120284999  //COMPONENT PEDS 
    {
        public MetaHash name { get; init; } //0   0: Hash: 0: name
        public ArrayOfBytes12 compDrawableId { get; init; } //4   4: ArrayOfBytes: 12: compDrawableId
        public ArrayOfBytes12 compTexId { get; init; } //16   16: ArrayOfBytes: 12: compTexId
        public ArrayOfBytes6 propAnchorId { get; init; } //28   28: ArrayOfBytes: 6: propAnchorId
        public ArrayOfBytes6 propDrawableId { get; init; } //34   34: ArrayOfBytes: 6: propDrawableId
        public ArrayOfBytes6 propTexId { get; init; } //40   40: ArrayOfBytes: 6: propTexId
        public ushort Unused0 { get; init; }//46
    }
    [TC(typeof(EXP))] public class MCPedSelectionSet : MetaWrapper
    {
        public MCPedVariationInfo Owner { get; set; }

        public CPedSelectionSet _Data;
        public CPedSelectionSet Data => _Data;

        public MCPedSelectionSet() { }
        public MCPedSelectionSet(Meta meta, CPedSelectionSet data, MCPedVariationInfo owner)
        {
            _Data = data;
            Owner = owner;
        }

        public override void Load(Meta meta, in MetaPOINTER ptr)
        {
            throw new NotImplementedException();
        }

        public override MetaPOINTER Save(MetaBuilder mb)
        {
            throw new NotImplementedException();
        }
    }

    [TC(typeof(EXP))]
    public readonly struct CComponentInfo //48 bytes, Key:3693847250  //COMPONENT PEDS 
    {
        public MetaHash pedXml_audioID { get; init; } //0   0: Hash: 0: 802196719
        public MetaHash pedXml_audioID2 { get; init; } //4   4: Hash: 0: 4233133352
        public ArrayOfBytes5 pedXml_expressionMods { get; init; } //8   8: ArrayOfBytes: 5: 128864925
        public byte Unused0 { get; init; }//13
        public ushort Unused1 { get; init; }//14
        public uint Unused2 { get; init; }//16
        public uint Unused3 { get; init; }//20
        public uint Unused4 { get; init; }//24
        public uint flags { get; init; } //28   28: UnsignedInt: 0: flags
        public int inclusions { get; init; } //32   32: IntFlags2: 0: inclusions//2172318933
        public int exclusions { get; init; } //36   36: IntFlags2: 0: exclusions
        public ePedVarComp pedXml_vfxComps { get; init; } //40   40: ShortFlags: ePedVarComp: 1613922652
        public ushort pedXml_flags { get; init; } //42   42: UnsignedShort: 0: 2114993291
        public byte pedXml_compIdx { get; init; } //44   44: UnsignedByte: 0: 3509540765
        public byte pedXml_drawblIdx { get; init; } //45   45: UnsignedByte: 0: 4196345791
        public ushort Unused5 { get; init; }//46
    }
    [TC(typeof(EXP))]
    public class MCComponentInfo : MetaWrapper
    {
        public MCPedVariationInfo Owner { get; set; }

        public CComponentInfo _Data;
        public CComponentInfo Data => _Data;


        public int ComponentType => _Data.pedXml_compIdx;
        public int ComponentIndex => _Data.pedXml_drawblIdx;

        public MCComponentInfo() { }
        public MCComponentInfo(Meta meta, in CComponentInfo data, MCPedVariationInfo owner)
        {
            _Data = data;
            Owner = owner;
        }


        public override void Load(Meta meta, in MetaPOINTER ptr)
        {
            throw new NotImplementedException();
        }

        public override MetaPOINTER Save(MetaBuilder mb)
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return (ComponentType < 12) ? MCPVComponentData.ComponentTypeNames[ComponentType] + "_" + ComponentIndex.ToString("000") : base.ToString();
        }
    }

    [TC(typeof(EXP))] public struct CPedPropInfo //40 bytes, Key:1792487819  //COMPONENT PEDS 
    {
        public byte numAvailProps { get; set; } //0   0: UnsignedByte: 0: numAvailProps
        public byte Unused0 { get; set; }//1
        public ushort Unused1 { get; set; }//2
        public uint Unused2 { get; set; }//4
        public Array_Structure aPropMetaData; //8   8: Array: 0: aPropMetaData  {0: Structure: CPedPropMetaData: 256}
        public Array_Structure aAnchors; //24   24: Array: 0: aAnchors  {0: Structure: CAnchorProps: 256}
        public CPedPropInfo SwapEnd()
        {
            aPropMetaData = aPropMetaData.SwapEnd();
            aAnchors = aAnchors.SwapEnd();
            return this;
        }
    }
    [TC(typeof(EXP))] public class MCPedPropInfo : MetaWrapper
    {
        public MCPedVariationInfo Owner { get; set; }

        public CPedPropInfo _Data;
        public CPedPropInfo Data => _Data;

        public MCPedPropMetaData[] PropMetaData { get; set; }
        public MCAnchorProps[] Anchors { get; set; }

        public MCPedPropInfo() { }
        public MCPedPropInfo(Meta meta, in CPedPropInfo data, MCPedVariationInfo owner)
        {
            _Data = data;
            Owner = owner;

            var vPropMetaData = MetaTypes.ConvertDataArray<CPedPropMetaData>(meta, MetaName.CPedPropMetaData, in _Data.aPropMetaData);
            if (vPropMetaData != null)
            {
                PropMetaData = new MCPedPropMetaData[vPropMetaData.Length];
                for (int i = 0; i < vPropMetaData.Length; i++)
                {
                    PropMetaData[i] = new MCPedPropMetaData(meta, in vPropMetaData[i], this);
                }
            }

            var vAnchors = MetaTypes.ConvertDataArray<CAnchorProps>(meta, MetaName.CAnchorProps, in _Data.aAnchors);
            if (vAnchors != null)
            {
                Anchors = new MCAnchorProps[vAnchors.Length];
                for (int i = 0; i < vAnchors.Length; i++)
                {
                    Anchors[i] = new MCAnchorProps(meta, vAnchors[i], this);
                }
            }

        }



        public override void Load(Meta meta, in MetaPOINTER ptr)
        {
            throw new NotImplementedException();
        }

        public override MetaPOINTER Save(MetaBuilder mb)
        {
            throw new NotImplementedException();
        }
    }

    [TC(typeof(EXP))]
    public struct CPedPropMetaData //56 bytes, Key:2029738350  //COMPONENT PEDS 
    {
        public MetaHash audioId { get; set; } //0   0: Hash: 0: audioId
        public ArrayOfBytes5 expressionMods { get; set; } //4   4: ArrayOfBytes: 5: expressionMods//942761829
        public byte Unused0 { get; set; }//9
        public ushort Unused1 { get; set; }//10
        public uint Unused2 { get; set; }//12
        public uint Unused3 { get; set; }//16
        public uint Unused4 { get; set; }//20
        public Array_Structure texData { get; set; } //24   24: Array: 0: texData  {0: Structure: CPedPropTexData: 256}
        public ePropRenderFlags renderFlags { get; set; } //40   40: IntFlags1: ePropRenderFlags: renderFlags
        public uint propFlags { get; set; } //44   44: UnsignedInt: 0: propFlags
        public ushort flags { get; set; } //48   48: UnsignedShort: 0: flags
        public byte anchorId { get; set; } //50   50: UnsignedByte: 0: anchorId
        public byte propId { get; set; } //51   51: UnsignedByte: 0: propId
        public byte stickyness { get; set; } //52   52: UnsignedByte: 0: 2894625425
        public byte Unused5 { get; set; }//53
        public ushort Unused6 { get; set; }//54
    }
    [TC(typeof(EXP))] public class MCPedPropMetaData : MetaWrapper
    {
        public MCPedPropInfo Owner { get; set; }

        public CPedPropMetaData _Data;
        public CPedPropMetaData Data => _Data;

        public CPedPropTexData[] TexData { get; set; }

        public MCPedPropMetaData(Meta meta, in CPedPropMetaData data, MCPedPropInfo owner)
        {
            _Data = data;
            Owner = owner;

            TexData = MetaTypes.ConvertDataArray<CPedPropTexData>(meta, MetaName.CPedPropTexData, _Data.texData);
        }

        public override void Load(Meta meta, in MetaPOINTER ptr)
        {
            throw new NotImplementedException();
        }

        public override MetaPOINTER Save(MetaBuilder mb)
        {
            throw new NotImplementedException();
        }
    }

    [TC(typeof(EXP))]
    public struct CPedPropTexData //12 bytes, Key:2767296137  //COMPONENT PEDS 
    {
        public int inclusions { get; set; } //0   0: IntFlags2: 0: inclusions
        public int exclusions { get; set; } //4   4: IntFlags2: 0: exclusions
        public byte texId { get; set; } //8   8: UnsignedByte: 0: texId
        public byte inclusionId { get; set; } //9   9: UnsignedByte: 0: inclusionId
        public byte exclusionId { get; set; } //10   10: UnsignedByte: 0: exclusionId
        public byte distribution { get; set; } //11   11: UnsignedByte: 0: distribution
    }

    [TC(typeof(EXP))]
    public struct CAnchorProps //24 bytes, Key:403574180  //COMPONENT PEDS CAnchorProps
    {
        public readonly Array_byte props; //0   0: Array: 0: props  {0: UnsignedByte: 0: 256}
        public eAnchorPoints anchor { get; set; } //16   16: IntEnum: eAnchorPoints: anchor
        public uint Unused0 { get; set; }//20
    }
    [TC(typeof(EXP))]
    public class MCAnchorProps : MetaWrapper
    {
        public MCPedPropInfo Owner { get; set; }

        public CAnchorProps _Data;
        public CAnchorProps Data => _Data;

        public byte[] Props { get; set; }

        public MCAnchorProps(Meta meta, CAnchorProps data, MCPedPropInfo owner)
        {
            _Data = data;
            Owner = owner;

            Props = MetaTypes.GetByteArray(meta, in _Data.props);
        }


        public override void Load(Meta meta, in MetaPOINTER ptr)
        {
            throw new NotImplementedException();
        }

        public override MetaPOINTER Save(MetaBuilder mb)
        {
            throw new NotImplementedException();
        }
    }









}
