using CodeWalker.GameFiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TC = System.ComponentModel.TypeConverterAttribute;
using EXP = System.ComponentModel.ExpandableObjectConverter;
using System.Xml;
using System.IO;

namespace CodeWalker.Core.GameFiles.FileTypes
{
    [TC(typeof(EXP))]
    public class PedShopMetaFile : GameFile, PackedFile
    {
        public static XmlNameTableThreadSafe nameTable = new XmlNameTableThreadSafe();
        public string PedName { get; set; }
        public string DlcName { get; set; }
        public string FullDlcName { get; set; }
        public string Character { get; set; }
        public string CreateMetaData { get; set; }

        public PedShopMetaFile() : base(null, GameFileType.PedShopMeta)
        { }
        public PedShopMetaFile(RpfFileEntry entry) : base(entry, GameFileType.PedShopMeta)
        { }

        public void Load(byte[] data, RpfFileEntry entry)
        {
            var xml = TextUtil.GetUTF8Text(data);

            using var xmlReader = XmlReader.Create(new StringReader(xml), new XmlReaderSettings { NameTable = nameTable });

            Load(xmlReader);
        }

        public void Load(XmlReader reader)
        {
            reader.MoveToStartElement("ShopPedApparel");
            var startDepth = reader.Depth;
            reader.ReadStartElement("ShopPedApparel");
            while (reader.Read() && startDepth < reader.Depth)
            {
                switch(reader.MoveToContent())
                {
                    case XmlNodeType.Element:
                        LoadElement(reader);
                        break;
                    case XmlNodeType.EndElement:
                        if (reader.Name == "ShopPedApparel")
                        {
                            return;
                        }
                        break;
                }
            }
        }

        public void LoadElement(XmlReader reader)
        {
            switch(reader.Name)
            {
                case "pedName":
                    PedName = reader.ReadElementContentAsString();
                    if (!string.IsNullOrEmpty(PedName))
                    {
                        JenkIndex.EnsureBoth(PedName);
                    }
                    break;
                case "dlcName":
                    DlcName = reader.ReadElementContentAsString();
                    if (!string.IsNullOrEmpty(DlcName))
                    {
                        JenkIndex.EnsureBoth(DlcName);
                    }
                    break;
                case "fullDlcName":
                    FullDlcName = reader.ReadElementContentAsString();
                    if (!string.IsNullOrEmpty(FullDlcName))
                    {
                        JenkIndex.EnsureBoth(FullDlcName);
                    }
                    break;
                case "eCharacter":
                    Character = reader.ReadElementContentAsString();
                    if (!string.IsNullOrEmpty(Character))
                    {
                        JenkIndex.EnsureBoth(Character);
                    }
                    break;
                case "createMetaData":
                    CreateMetaData = reader.ReadElementContentAsString();
                    if (!string.IsNullOrEmpty(CreateMetaData))
                    {
                        JenkIndex.EnsureBoth(CreateMetaData);
                    }
                    break;
                case "pedOutfits":
                    reader.Skip();
                    break;
                case "pedComponents":
                    reader.Skip();
                    break;
                case "pedProps":
                    reader.Skip();
                    break;

            }
        }
    }
}
