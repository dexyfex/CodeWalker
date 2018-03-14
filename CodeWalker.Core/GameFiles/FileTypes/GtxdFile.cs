﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace CodeWalker.GameFiles
{
    [TypeConverter(typeof(ExpandableObjectConverter))] public class GtxdFile : GameFile, PackedFile
    {

        public RbfFile Rbf { get; set; }


        public Dictionary<string, string> CMapParentTxds { get; set; }




        public GtxdFile() : base(null, GameFileType.Gtxd)
        {
        }
        public GtxdFile(RpfFileEntry entry) : base(entry, GameFileType.Gtxd)
        {
        }



        public void Load(byte[] data, RpfFileEntry entry)
        {
            RpfFileEntry = entry;
            Name = entry.Name;
            FilePath = Name;


            if (entry.NameLower.EndsWith(".ymt"))
            {
                MemoryStream ms = new MemoryStream(data);
                if (RbfFile.IsRBF(ms))
                {
                    Rbf = new RbfFile();
                    var rbfstruct = Rbf.Load(ms);

                    if (rbfstruct.Name == "CMapParentTxds")
                    {
                        LoadMapParentTxds(rbfstruct);
                    }

                    Loaded = true;
                    return;
                }
                else
                {
                    //not an RBF file...
                }
            }
            else if (entry.NameLower.EndsWith(".meta"))
            {
                //required for update\x64\dlcpacks\mpheist\dlc.rpf\common\data\gtxd.meta and update\x64\dlcpacks\mpluxe\dlc.rpf\common\data\gtxd.meta
                string bom = Encoding.UTF8.GetString(Encoding.UTF8.GetPreamble());
                string xml = Encoding.UTF8.GetString(data);

                if (xml.StartsWith(bom, StringComparison.Ordinal))
                {
                    xml = xml.Remove(0, bom.Length);
                }

                LoadMapParentTxds(xml);
                Loaded = true;
            }


        }


        private void LoadMapParentTxds(RbfStructure rbfstruct)
        {

            CMapParentTxds = new Dictionary<string, string>();
            //StringBuilder sblist = new StringBuilder();
            foreach (var child in rbfstruct.Children)
            {
                var childstruct = child as RbfStructure;
                if ((childstruct != null) && (childstruct.Name == "txdRelationships"))
                {
                    foreach (var txdrel in childstruct.Children)
                    {
                        var txdrelstruct = txdrel as RbfStructure;
                        if ((txdrelstruct != null) && (txdrelstruct.Name == "item"))
                        {
                            string parentstr = string.Empty;
                            string childstr = string.Empty;
                            foreach (var item in txdrelstruct.Children)
                            {
                                var itemstruct = item as RbfStructure;
                                if ((itemstruct != null))
                                {
                                    var strbytes = itemstruct.Children[0] as RbfBytes;
                                    string thisstr = string.Empty;
                                    if (strbytes != null)
                                    {
                                        thisstr = Encoding.ASCII.GetString(strbytes.Value).Replace("\0", "");
                                    }
                                    switch (item.Name)
                                    {
                                        case "parent":
                                            parentstr = thisstr;
                                            break;
                                        case "child":
                                            childstr = thisstr;
                                            break;
                                    }
                                }

                            }
                            if ((!string.IsNullOrEmpty(parentstr)) && (!string.IsNullOrEmpty(childstr)))
                            {
                                if (!CMapParentTxds.ContainsKey(childstr))
                                {
                                    CMapParentTxds.Add(childstr, parentstr);
                                }
                                else
                                {
                                }
                                //sblist.AppendLine(childstr + ": " + parentstr);
                            }
                        }
                    }
                }
            }
            //string alltxdmap = sblist.ToString();
            //if (!string.IsNullOrEmpty(alltxdmap))
            //{
            //}

        }



        private void LoadMapParentTxds(string xml)
        {
            XmlDocument xmldoc = new XmlDocument();
            xmldoc.LoadXml(xml); //maybe better load xml.ToLower() and use "cmapparenttxds/txdrelationships/item" as xpath?
            XmlNodeList items = xmldoc.SelectNodes("CMapParentTxds/txdRelationships/Item | CMapParentTxds/txdRelationships/item");

            CMapParentTxds = new Dictionary<string, string>();
            for (int i = 0; i < items.Count; i++)
            {
                string parentstr = Xml.GetChildInnerText(items[i], "parent");
                string childstr = Xml.GetChildInnerText(items[i], "child");

                if ((!string.IsNullOrEmpty(parentstr)) && (!string.IsNullOrEmpty(childstr)))
                {
                    if (!CMapParentTxds.ContainsKey(childstr))
                    {
                        CMapParentTxds.Add(childstr, parentstr);
                    }
                }
            }
        }

    }
}
