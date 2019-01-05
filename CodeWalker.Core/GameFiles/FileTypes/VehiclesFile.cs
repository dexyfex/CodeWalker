using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace CodeWalker.GameFiles
{
    public class VehiclesFile : GameFile, PackedFile
    {

        public RbfFile Rbf { get; set; }


        public Dictionary<string, string> TxdRelationships { get; set; }




        public VehiclesFile() : base(null, GameFileType.Vehicles)
        {
        }
        public VehiclesFile(RpfFileEntry entry) : base(entry, GameFileType.Vehicles)
        {
        }



        public void Load(byte[] data, RpfFileEntry entry)
        {
            RpfFileEntry = entry;
            Name = entry.Name;
            FilePath = Name;


            if (entry.NameLower.EndsWith(".meta"))
            {
                //required for update\x64\dlcpacks\mpheist\dlc.rpf\common\data\gtxd.meta and update\x64\dlcpacks\mpluxe\dlc.rpf\common\data\gtxd.meta
                string bom = Encoding.UTF8.GetString(Encoding.UTF8.GetPreamble());
                string xml = Encoding.UTF8.GetString(data);

                if (xml.StartsWith(bom, StringComparison.Ordinal))
                {
                    xml = xml.Remove(0, bom.Length);
                }

                XmlDocument xmldoc = new XmlDocument();
                xmldoc.LoadXml(xml);

                LoadVehicles(xmldoc);

                LoadTxdRelationships(xmldoc);

                Loaded = true;
            }
        }


        private void LoadVehicles(XmlDocument xmldoc)
        {
            XmlNodeList items = xmldoc.SelectNodes("CVehicleModelInfo__InitDataList/InitDatas/Item | CVehicleModelInfo__InitDataList/InitDatas/item");
            for (int i = 0; i < items.Count; i++)
            {
                //TODO...
            }
        }

        private void LoadTxdRelationships(XmlDocument xmldoc)
        {
            XmlNodeList items = xmldoc.SelectNodes("CVehicleModelInfo__InitDataList/txdRelationships/Item | CVehicleModelInfo__InitDataList/txdRelationships/item");

            TxdRelationships = new Dictionary<string, string>();
            for (int i = 0; i < items.Count; i++)
            {
                string parentstr = Xml.GetChildInnerText(items[i], "parent");
                string childstr = Xml.GetChildInnerText(items[i], "child");

                if ((!string.IsNullOrEmpty(parentstr)) && (!string.IsNullOrEmpty(childstr)))
                {
                    if (!TxdRelationships.ContainsKey(childstr))
                    {
                        TxdRelationships.Add(childstr, parentstr);
                    }
                }
            }
        }


    }
}
