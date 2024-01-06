using Collections.Pooled;
using CommunityToolkit.HighPerformance;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace CodeWalker.GameFiles
{
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class GtxdFile : GameFile, PackedFile
    {
        public RbfFile Rbf { get; set; }

        public PooledDictionary<string, string> TxdRelationships { get; set; }

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


            if (entry.IsExtension(".ymt"))
            {
                if (RbfFile.IsRBF(data.AsSpan(0, 4)))
                {
                    //using MemoryStream ms = new MemoryStream(data);
                    var sequence = new ReadOnlySequence<byte>(data);
                    var reader = new SequenceReader<byte>(sequence);
                    Rbf = new RbfFile();
                    var rbfstruct = Rbf.Load(ref reader);

                    if (rbfstruct.Name == "CMapParentTxds")
                    {
                        LoadTxdRelationships(rbfstruct);
                    }

                    Loaded = true;
                    return;
                }
            }
            else if (entry.IsExtension(".meta"))
            {
                //required for update\x64\dlcpacks\mpheist\dlc.rpf\common\data\gtxd.meta and update\x64\dlcpacks\mpluxe\dlc.rpf\common\data\gtxd.meta
                string xml = TextUtil.GetUTF8Text(data);

                LoadTxdRelationships(xml);
                Loaded = true;
            }


        }


        private void LoadTxdRelationships(RbfStructure rbfstruct)
        {

            TxdRelationships?.Clear();
            if (rbfstruct.Children is null)
                return;

            TxdRelationships ??= new PooledDictionary<string, string>();
            //StringBuilder sblist = new StringBuilder();
            foreach (var child in rbfstruct.Children.Span)
            {
                if (child is RbfStructure childstruct && childstruct.Name == "txdRelationships" && childstruct.Children is not null)
                {
                    TxdRelationships.EnsureCapacity(TxdRelationships.Count + childstruct.Children.Count);
                    foreach (var txdrel in childstruct.Children)
                    {
                        if (txdrel is RbfStructure txdrelstruct && txdrelstruct.Name == "item" && txdrelstruct.Children is not null)
                        {
                            string parentstr = string.Empty;
                            string childstr = string.Empty;
                            foreach (var item in txdrelstruct.Children)
                            {
                                if (item is RbfStructure itemstruct)
                                {
                                    string thisstr = string.Empty;
                                    if (itemstruct.Children is not null && itemstruct.Children.Count > 0 && itemstruct.Children[0] is RbfBytes strbytes)
                                    {
                                        thisstr = strbytes.GetNullTerminatedString();
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
                            if (!string.IsNullOrEmpty(parentstr) && !string.IsNullOrEmpty(childstr))
                            {
                                _ = TxdRelationships.TryAdd(childstr, parentstr);
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



        private void LoadTxdRelationships(string xml)
        {
            XmlDocument xmldoc = new XmlDocument();
            xmldoc.LoadXml(xml); //maybe better load xml.ToLower() and use "cmapparenttxds/txdrelationships/item" as xpath?
            XmlNodeList items = xmldoc.SelectNodes("CMapParentTxds/txdRelationships/Item | CMapParentTxds/txdRelationships/item");

            TxdRelationships = new PooledDictionary<string, string>(items.Count);
            for (int i = 0; i < items.Count; i++)
            {
                string parentstr = Xml.GetChildInnerText(items[i], "parent");
                string childstr = Xml.GetChildInnerText(items[i], "child");

                if (!string.IsNullOrEmpty(parentstr) && !string.IsNullOrEmpty(childstr))
                {
                    _ = TxdRelationships.TryAdd(childstr, parentstr);
                }
            }
        }

        public override void Dispose()
        {
            TxdRelationships?.Dispose();
            Rbf?.Dispose();
            GC.SuppressFinalize(this);
            base.Dispose();
        }

    }
}
