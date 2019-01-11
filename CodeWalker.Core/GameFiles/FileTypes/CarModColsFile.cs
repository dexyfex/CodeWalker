using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

using TC = System.ComponentModel.TypeConverterAttribute;
using EXP = System.ComponentModel.ExpandableObjectConverter;

namespace CodeWalker.GameFiles
{
    [TC(typeof(EXP))] public class CarModColsFile : GameFile, PackedFile
    {
        public PsoFile Pso { get; set; }
        public string Xml { get; set; }

        public CVehicleModColours VehicleModColours { get; set; }

        public CarModColsFile() : base(null, GameFileType.CarModCols)
        { }
        public CarModColsFile(RpfFileEntry entry) : base(entry, GameFileType.CarModCols)
        {
        }

        public void Load(byte[] data, RpfFileEntry entry)
        {
            RpfFileEntry = entry;
            Name = entry.Name;
            FilePath = Name;


            //always PSO .ymt
            MemoryStream ms = new MemoryStream(data);
            if (PsoFile.IsPSO(ms))
            {
                Pso = new PsoFile();
                Pso.Load(data);
                Xml = PsoXml.GetXml(Pso); //yep let's just convert that to XML :P
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
                VehicleModColours = new CVehicleModColours(xdoc.DocumentElement);
            }




            Loaded = true;
        }
    }

    [TC(typeof(EXP))] public class CVehicleModColours
    {
        public CVehicleModColor[] metallic { get; set; }
        public CVehicleModColor[] classic { get; set; }
        public CVehicleModColor[] matte { get; set; }
        public CVehicleModColor[] metals { get; set; }
        public CVehicleModColor[] chrome { get; set; }
        public CVehicleModPearlescentColors pearlescent { get; set; }

        public CVehicleModColours(XmlNode node)
        {
            XmlNode cnode;
            cnode = node.SelectSingleNode("metallic");
            if (cnode != null)
            {
                var items = cnode.SelectNodes("Item");
                if (items.Count > 0)
                {
                    metallic = new CVehicleModColor[items.Count];
                    for (int i = 0; i < items.Count; i++)
                    {
                        metallic[i] = new CVehicleModColor(items[i]);
                    }
                }
            }
            cnode = node.SelectSingleNode("classic");
            if (cnode != null)
            {
                var items = cnode.SelectNodes("Item");
                if (items.Count > 0)
                {
                    classic = new CVehicleModColor[items.Count];
                    for (int i = 0; i < items.Count; i++)
                    {
                        classic[i] = new CVehicleModColor(items[i]);
                    }
                }
            }
            cnode = node.SelectSingleNode("matte");
            if (cnode != null)
            {
                var items = cnode.SelectNodes("Item");
                if (items.Count > 0)
                {
                    matte = new CVehicleModColor[items.Count];
                    for (int i = 0; i < items.Count; i++)
                    {
                        matte[i] = new CVehicleModColor(items[i]);
                    }
                }
            }
            cnode = node.SelectSingleNode("metals");
            if (cnode != null)
            {
                var items = cnode.SelectNodes("Item");
                if (items.Count > 0)
                {
                    metals = new CVehicleModColor[items.Count];
                    for (int i = 0; i < items.Count; i++)
                    {
                        metals[i] = new CVehicleModColor(items[i]);
                    }
                }
            }
            cnode = node.SelectSingleNode("chrome");
            if (cnode != null)
            {
                var items = cnode.SelectNodes("Item");
                if (items.Count > 0)
                {
                    chrome = new CVehicleModColor[items.Count];
                    for (int i = 0; i < items.Count; i++)
                    {
                        chrome[i] = new CVehicleModColor(items[i]);
                    }
                }
            }
            cnode = node.SelectSingleNode("pearlescent");
            if (cnode != null)
            {
                pearlescent = new CVehicleModPearlescentColors(cnode);
            }

        }
    }
    [TC(typeof(EXP))] public class CVehicleModColor
    {
        public string name { get; set; }
        public byte col { get; set; }
        public byte spec { get; set; }

        public CVehicleModColor(XmlNode node)
        {
            name = Xml.GetChildInnerText(node, "name");
            col = (byte)Xml.GetChildIntAttribute(node, "col", "value");
            spec = (byte)Xml.GetChildIntAttribute(node, "spec", "value");
        }
        public override string ToString()
        {
            return name;
        }
    }
    [TC(typeof(EXP))] public class CVehicleModPearlescentColors
    {
        public CVehicleModColor[] baseCols { get; set; }
        public CVehicleModColor[] specCols { get; set; }

        public CVehicleModPearlescentColors(XmlNode node)
        {
            XmlNode cnode;
            cnode = node.SelectSingleNode("baseCols");
            if (cnode != null)
            {
                var items = cnode.SelectNodes("Item");
                if (items.Count > 0)
                {
                    baseCols = new CVehicleModColor[items.Count];
                    for (int i = 0; i < items.Count; i++)
                    {
                        baseCols[i] = new CVehicleModColor(items[i]);
                    }
                }
            }
            cnode = node.SelectSingleNode("specCols");
            if (cnode != null)
            {
                var items = cnode.SelectNodes("Item");
                if (items.Count > 0)
                {
                    specCols = new CVehicleModColor[items.Count];
                    for (int i = 0; i < items.Count; i++)
                    {
                        specCols[i] = new CVehicleModColor(items[i]);
                    }
                }
            }
        }
    }
}
