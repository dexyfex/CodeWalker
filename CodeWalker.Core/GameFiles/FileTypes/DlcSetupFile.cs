using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace CodeWalker.GameFiles
{
    public class DlcSetupFile
    {
        public string deviceName { get; set; }
        public string datFile { get; set; }
        public string nameHash { get; set; }
        public List<DlcSetupContentChangesetGroup> contentChangeSetGroups { get; set; }
        public string type { get; set; }
        public string timeStamp { get; set; }
        public int order { get; set; }
        public int minorOrder { get; set; }
        public int subPackCount { get; set; }
        public bool isLevelPack { get; set; }

        public RpfFile DlcFile { get; set; } //used by GameFileCache
        public DlcContentFile ContentFile { get; set; }

        public void Load(XmlDocument doc)
        {

            var root = doc.DocumentElement;
            deviceName = Xml.GetChildInnerText(root, "deviceName");
            datFile = Xml.GetChildInnerText(root, "datFile");
            nameHash = Xml.GetChildInnerText(root, "nameHash");
            type = Xml.GetChildInnerText(root, "type");
            timeStamp = Xml.GetChildInnerText(root, "timeStamp");
            order = Xml.GetIntAttribute(root.SelectSingleNode("order"), "value");
            minorOrder = Xml.GetIntAttribute(root.SelectSingleNode("minorOrder"), "value");
            subPackCount = Xml.GetIntAttribute(root.SelectSingleNode("subPackCount"), "value");
            isLevelPack = Xml.GetBoolAttribute(root.SelectSingleNode("isLevelPack"), "value");

            contentChangeSetGroups = new List<DlcSetupContentChangesetGroup>();
            var groups = root.SelectNodes("contentChangeSetGroups/Item");
            foreach (XmlNode node in groups)
            {
                var group = new DlcSetupContentChangesetGroup();
                group.Load(node);
                contentChangeSetGroups.Add(group);
            }

            if (root.ChildNodes.Count > 15)
            { }
        }

        public override string ToString()
        {
            return deviceName + ", " + datFile + ", " + nameHash + ", " + type + ", " + order.ToString() + ", " + ((contentChangeSetGroups != null) ? contentChangeSetGroups.Count.ToString() : "0") + " groups, " + timeStamp;
        }
    }

    public class DlcSetupContentChangesetGroup
    {
        public string NameHash { get; set; }
        public List<string> ContentChangeSets { get; set; }

        public void Load(XmlNode node)
        {
            if (node.ChildNodes.Count != 2)
            { }
            NameHash = Xml.GetChildInnerText(node, "NameHash");
            ContentChangeSets = new List<string>();
            var changesets = node.SelectNodes("ContentChangeSets/Item");
            foreach (XmlNode changeset in changesets)
            {
                ContentChangeSets.Add(changeset.InnerText);
            }
        }

        public override string ToString()
        {
            return NameHash + " (" + ((ContentChangeSets != null) ? ContentChangeSets.Count.ToString() : "0") + " changesets)";
        }
    }
}
