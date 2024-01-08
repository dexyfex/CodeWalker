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
using CodeWalker.Core.Utils;

namespace CodeWalker.GameFiles
{
    [TC(typeof(EXP))] public class CarVariationsFile : GameFile, PackedFile
    {
        public PsoFile Pso { get; set; }
        public string Xml { get; set; }

        public CVehicleModelInfoVariation VehicleModelInfo { get; set; }

        public CarVariationsFile() : base(null, GameFileType.CarVariations)
        { }
        public CarVariationsFile(RpfFileEntry entry) : base(entry, GameFileType.CarVariations)
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
                Xml = TextUtil.GetUTF8Text(data);
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
                VehicleModelInfo = new CVehicleModelInfoVariation(xdoc.DocumentElement);
            }


            Loaded = true;
        }
    }

    [TC(typeof(EXP))] public class CVehicleModelInfoVariation
    {
        public CVehicleModelInfoVariation_418053801[] variationData { get; set; }

        public CVehicleModelInfoVariation(XmlNode node)
        {
            var variationNode = node.SelectSingleNode("variationData");
            if (variationNode is not null)
            {
                var items = variationNode.SelectNodes("Item");
                if (items is not null && items.Count > 0)
                {
                    variationData = new CVehicleModelInfoVariation_418053801[items.Count];
                    for (int i = 0; i < items.Count; i++)
                    {
                        variationData[i] = new CVehicleModelInfoVariation_418053801(items[i]);
                    }
                }
            }
        }
    }
    [TC(typeof(EXP))] public class CVehicleModelInfoVariation_418053801
    {
        public string modelName { get; set; }
        public CVehicleModelInfoVariation_2575850962[] colors { get; set; }
        public MetaHash[] kits { get; set; }
        public MetaHash[] windowsWithExposedEdges { get; set; }
        public PlateProbabilities plateProbabilities { get; set; }
        public byte lightSettings { get; set; }
        public byte sirenSettings { get; set; }

        public CVehicleModelInfoVariation_418053801(XmlNode node)
        {
            modelName = Xml.GetChildInnerText(node, "modelName");
            var colorsNode = node.SelectSingleNode("colors");
            if (colorsNode is not null)
            {
                var items = colorsNode.SelectNodes("Item");
                if (items is not null && items.Count > 0)
                {
                    colors = new CVehicleModelInfoVariation_2575850962[items.Count];
                    for (int i = 0; i < items.Count; i++)
                    {
                        colors[i] = new CVehicleModelInfoVariation_2575850962(items[i]);
                    }
                }
            }
            var kitsNode = node.SelectSingleNode("kits");
            if (kitsNode is not null)
            {
                var items = kitsNode.SelectNodes("Item");
                if (items is not null && items.Count > 0)
                {
                    kits = new MetaHash[items.Count];
                    for (int i = 0; i < items.Count; i++)
                    {
                        kits[i] = XmlMeta.GetHash(items[i].InnerText);
                    }
                }
            }
            var windowsNodes = node.SelectSingleNode("windowsWithExposedEdges");
            if (windowsNodes is not null)
            {
                var items = windowsNodes.SelectNodes("Item");
                if (items is not null && items.Count > 0)
                {
                    windowsWithExposedEdges = new MetaHash[items.Count];
                    for (int i = 0; i < items.Count; i++)
                    {
                        windowsWithExposedEdges[i] = XmlMeta.GetHash(items[i].InnerText);
                    }
                }
            }
            var plateProbabilitiesNode = node.SelectSingleNode("plateProbabilities");
            if (plateProbabilitiesNode is not null)
            {
                plateProbabilities = new PlateProbabilities(plateProbabilitiesNode);
            }
            lightSettings = (byte)Xml.GetChildIntAttribute(node, "lightSettings", "value");
            sirenSettings = (byte)Xml.GetChildIntAttribute(node, "sirenSettings", "value");
        }

        public override string ToString()
        {
            return modelName;
        }
    }
    [TC(typeof(EXP))] public class CVehicleModelInfoVariation_2575850962
    {
        public byte[] indices { get; set; }
        public bool[] liveries { get; set; }

        public CVehicleModelInfoVariation_2575850962(XmlNode node)
        {
            var indicesNode = node.SelectSingleNode("indices");
            if (indicesNode is not null)
            {
                var astr = indicesNode.InnerText;
                var alist = new List<byte>();
                foreach (var item in astr.EnumerateSplitAny(['\n', ' ', '\t']))
                {
                    var titem = item.Trim();
                    if (byte.TryParse(titem, out var v))
                    {
                        alist.Add(v);
                    }
                }
                indices = alist.ToArray();
            }
            var liveriesNode = node.SelectSingleNode("liveries");
            if (liveriesNode is not null)
            {
                var items = liveriesNode.SelectNodes("Item");
                if (items.Count > 0)
                {
                    liveries = new bool[items.Count];
                    for (int i = 0; i < items.Count; i++)
                    {
                        liveries[i] = Xml.GetBoolAttribute(items[i], "value");
                    }
                }
                else
                {
                    var astr = liveriesNode.InnerText;
                    var alist = new List<bool>();
                    foreach (var item in astr.EnumerateSplitAny(['\n', ' ', '\t']))
                    {
                        var titem = item.Trim();
                        if (byte.TryParse(titem, out var v))
                        {
                            alist.Add(v > 0);
                        }
                    }
                    liveries = alist.ToArray();
                }
            }


        }
    }
    [TC(typeof(EXP))] public class PlateProbabilities
    {
        public PlateProbabilities_938618322[] Probabilities { get; set; }

        public PlateProbabilities(XmlNode node)
        {
            XmlNode cnode;
            cnode = node.SelectSingleNode("Probabilities");
            if (cnode != null)
            {
                var items = cnode.SelectNodes("Item");
                if (items.Count > 0)
                {
                    Probabilities = new PlateProbabilities_938618322[items.Count];
                    for (int i = 0; i < items.Count; i++)
                    {
                        Probabilities[i] = new PlateProbabilities_938618322(items[i]);
                    }
                }
            }
        }
    }
    [TC(typeof(EXP))] public class PlateProbabilities_938618322
    {
        public MetaHash Name { get; set; }
        public uint Value { get; set; }

        public PlateProbabilities_938618322(XmlNode node)
        {
            Name = XmlMeta.GetHash(Xml.GetChildInnerText(node, "Name"));
            Value = Xml.GetChildUIntAttribute(node, "Value", "value");
        }

        public override string ToString()
        {
            return $"{Name}: {Value}";
        }
    }

}
