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
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class YftFile : GameFile, PackedFile
    {
        public FragType Fragment { get; set; }

        public YftFile() : base(null, GameFileType.Yft)
        {
        }
        public YftFile(RpfFileEntry entry) : base(entry, GameFileType.Yft)
        {
        }

        public void Load(byte[] data, RpfFileEntry entry)
        {
            Name = entry.Name;
            RpfFileEntry = entry;

            RpfResourceFileEntry resentry = entry as RpfResourceFileEntry;
            if (resentry == null)
            {
                throw new Exception("File entry wasn't a resource! (is it binary data?)");
            }

            ResourceDataReader rd = new ResourceDataReader(resentry, data);

            Fragment = rd.ReadBlock<FragType>();

            if (Fragment != null)
            {
                Fragment.Yft = this;

                if (Fragment.Drawable != null)
                {
                    Fragment.Drawable.Owner = this;
                }
                if (Fragment.Drawable2 != null)
                {
                    Fragment.Drawable2.Owner = this;
                }
            }

            Loaded = true;
        }

        public byte[] Save()
        {
            byte[] data = ResourceBuilder.Build(Fragment, 162); //yft is type/version 162...

            return data;
        }


    }





    public class YftXml : MetaXmlBase
    {

        public static string GetXml(YftFile yft, string outputFolder = "")
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(XmlHeader);

            var ddsfolder = outputFolder;
            if (!string.IsNullOrEmpty(ddsfolder))
            {
                ddsfolder = Path.Combine(outputFolder, yft.Name);

                bool hastxd = false;
                if (yft?.Fragment != null)
                {
                    hastxd = hastxd || (yft.Fragment.Drawable?.ShaderGroup?.TextureDictionary != null);
                    hastxd = hastxd || (yft.Fragment.Drawable2?.ShaderGroup?.TextureDictionary != null);
                    if (yft.Fragment.DrawableArray?.data_items != null)
                    {
                        foreach (var d in yft.Fragment.DrawableArray?.data_items)
                        {
                            if (hastxd) break;
                            if (d?.ShaderGroup?.TextureDictionary != null)
                            {
                                hastxd = true;
                            }
                        }
                    }
                    if (yft.Fragment.PhysicsLODGroup?.PhysicsLOD1?.Children?.data_items != null)
                    {
                        foreach (var child in yft.Fragment.PhysicsLODGroup.PhysicsLOD1.Children.data_items)
                        {
                            if (hastxd) break;
                            hastxd = hastxd || (child.Drawable1?.ShaderGroup?.TextureDictionary != null);
                            hastxd = hastxd || (child.Drawable2?.ShaderGroup?.TextureDictionary != null);
                        }
                    }
                }
                if (hastxd)
                {
                    if (!Directory.Exists(ddsfolder))
                    {
                        Directory.CreateDirectory(ddsfolder);
                    }
                }
            }

            if (yft?.Fragment != null)
            {
                FragType.WriteXmlNode(yft.Fragment, sb, 0, ddsfolder);
            }

            return sb.ToString();
        }

    }

    public class XmlYft
    {

        public static YftFile GetYft(string xml, string inputFolder = "")
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);
            return GetYft(doc, inputFolder);
        }

        public static YftFile GetYft(XmlDocument doc, string inputFolder = "")
        {
            YftFile r = new YftFile();

            var ddsfolder = inputFolder;

            var node = doc.DocumentElement;
            if (node != null)
            {
                r.Fragment = FragType.ReadXmlNode(node, ddsfolder);
            }

            r.Name = Path.GetFileName(inputFolder);

            return r;
        }

    }




}
