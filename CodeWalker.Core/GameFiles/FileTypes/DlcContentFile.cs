using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace CodeWalker.GameFiles
{
    public class DlcContentFile
    {
        public List<DlcContentDataFile> dataFiles { get; set; } = new List<DlcContentDataFile>();
        public List<DlcContentChangeSet> contentChangeSets { get; set; } = new List<DlcContentChangeSet>();

        public RpfFile DlcFile { get; set; } //used by GameFileCache
        public Dictionary<string, DlcExtraFolderMountFile> ExtraMounts { get; set; } = new Dictionary<string, DlcExtraFolderMountFile>(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, DlcContentDataFile> RpfDataFiles { get; set; } = new Dictionary<string, DlcContentDataFile>(StringComparer.OrdinalIgnoreCase);

        public DlcExtraTitleUpdateFile ExtraTitleUpdates { get; set; }

        public void Load(XmlReader reader)
        {
            reader.MoveToStartElement("CDataFileMgr__ContentsOfDataFileXml");
            while (reader.Read())
            {
                switch (reader.MoveToContent())
                {
                    case XmlNodeType.Element:
                        ReadElement(reader);
                        break;
                    case XmlNodeType.EndElement:
                        if (reader.Name == "CDataFileMgr__ContentsOfDataFileXml")
                        {
                            //Reached end of file
                            return;
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        public void ReadElement(XmlReader reader)
        {
            switch(reader.Name)
            {
                case "disabledFiles":
                case "includedXmlFiles":
                case "includedDataFiles":
                case "patchFiles":
                    reader.Skip();
                    return;

                case "dataFiles":
                    foreach(var item in reader.IterateItems("dataFiles"))
                    {
                        dataFiles.Add(new DlcContentDataFile(item));
                    }
                    return;

                case "contentChangeSets":
                    foreach(var item in reader.IterateItems("contentChangeSets"))
                    {
                        contentChangeSets.Add(new DlcContentChangeSet(item));
                    }
                    return;
            }
        }

        public void Load(XmlDocument doc)
        {

            var root = doc.DocumentElement;

            dataFiles.Clear();
            contentChangeSets.Clear();

            foreach (XmlNode node in root.ChildNodes)
            {
                switch (node.Name)
                {
                    case "disabledFiles":
                        //foreach (XmlNode disabledFile in node.ChildNodes)
                        //{ } //nothing to see here..
                        break;
                    case "includedXmlFiles":
                        //foreach (XmlNode includedXmlFile in node.ChildNodes)
                        //{ } //nothing to see here..
                        break;
                    case "includedDataFiles":
                        //foreach (XmlNode includedDataFile in node.ChildNodes)
                        //{ } //nothing to see here..
                        break;
                    case "dataFiles":
                        foreach (XmlNode dataFile in node.ChildNodes)
                        {
                            if (dataFile.NodeType == XmlNodeType.Element)
                            {
                                dataFiles.Add(new DlcContentDataFile(dataFile));
                            }
                        }
                        break;
                    case "contentChangeSets":
                        foreach (XmlNode contentChangeSet in node.ChildNodes)
                        {
                            if (contentChangeSet.NodeType == XmlNodeType.Element)
                            {
                                contentChangeSets.Add(new DlcContentChangeSet(contentChangeSet));
                            }
                        }
                        break;
                    case "patchFiles":
                        //foreach (XmlNode patchFile in node.ChildNodes)
                        //{ } //nothing to see here..
                        break;
                    default:
                        break;
                }


            }


        }

        public void LoadDicts(DlcSetupFile setupfile, RpfManager rpfman, GameFileCache gfc)
        {
            ExtraMounts.Clear();
            RpfDataFiles.Clear();

            foreach (var datafile in dataFiles)
            {
                string dlcPlatformPath = GameFileCache.GetDlcPlatformPath(datafile.filename);
                if (datafile.fileType == "EXTRA_FOLDER_MOUNT_DATA")
                {
                    string efmdxmlpath = datafile.filename.Replace($"{setupfile.deviceName}:", DlcFile.Path).Replace('/', '\\');
                    efmdxmlpath = gfc.GetDlcPatchedPath(efmdxmlpath);
                    XmlDocument efmdxml = rpfman.GetFileXml(efmdxmlpath);

                    DlcExtraFolderMountFile efmf = new DlcExtraFolderMountFile();
                    efmf.Load(efmdxml);

                    ExtraMounts[dlcPlatformPath] = efmf;
                }
                if (datafile.fileType == "EXTRA_TITLE_UPDATE_DATA")
                {
                    string etudxmlpath = datafile.filename.Replace($"{setupfile.deviceName}:", DlcFile.Path).Replace('/', '\\');
                    etudxmlpath = gfc.GetDlcPatchedPath(etudxmlpath);
                    XmlDocument etudxml = rpfman.GetFileXml(etudxmlpath);

                    DlcExtraTitleUpdateFile etuf = new DlcExtraTitleUpdateFile();
                    etuf.Load(etudxml);

                    ExtraTitleUpdates = etuf;
                }
                if (datafile.fileType == "RPF_FILE")
                {
                    RpfDataFiles[dlcPlatformPath] = datafile;
                }
            }

        }

        public override string ToString()
        {
            return $"{dataFiles.Count} dataFiles, {contentChangeSets.Count} contentChangeSets";
        }
    }

    public class DlcContentDataFile
    {
        public string filename { get; set; }
        public string fileType { get; set; }
        public string contents { get; set; }
        public string installPartition { get; set; }
        public bool overlay { get; set; }
        public bool disabled { get; set; }
        public bool persistent { get; set; }
        public bool loadCompletely { get; set; }
        public bool locked { get; set; }

        public DlcContentDataFile(XmlNode node)
        {
            Load(node);
        }

        public DlcContentDataFile(XElement xElement)
        {
            Load(xElement);
        }

        public void Load(XElement xElement)
        {
            foreach(var child in xElement.Elements())
            {
                switch(child.Name.LocalName)
                {
                    case "filename":
                        filename = child.Value;
                        break;
                    case "fileType":
                        fileType = child.Value;
                        break;
                    case "contents":
                        contents = child.Value;
                        break;
                    case "installPartition":
                        installPartition = child.Value;
                        break;
                    case "overlay":
                        overlay = child.GetBoolAttribute("value");
                        break;
                    case "disabled":
                        disabled = child.GetBoolAttribute("value");
                        break;
                    case "persistent":
                        persistent = child.GetBoolAttribute("value");
                        break;
                    case "loadCompletely":
                        loadCompletely = child.GetBoolAttribute("value");
                        break;
                    case "locked":
                        locked = child.GetBoolAttribute("value");
                        break;
                    default:
                        break;

                }
            }
        }

        public void Load(XmlNode node)
        {
            foreach (XmlNode child in node.ChildNodes)
            {
                switch (child.Name)
                {
                    case "filename":
                        filename = child.InnerText;
                        break;
                    case "fileType":
                        fileType = child.InnerText;
                        break;
                    case "contents":
                        contents = child.InnerText;
                        break;
                    case "installPartition":
                        installPartition = child.InnerText;
                        break;
                    case "overlay":
                        overlay = Xml.GetBoolAttribute(child, "value");
                        break;
                    case "disabled":
                        disabled = Xml.GetBoolAttribute(child, "value");
                        break;
                    case "persistent":
                        persistent = Xml.GetBoolAttribute(child, "value");
                        break;
                    case "loadCompletely":
                        loadCompletely = Xml.GetBoolAttribute(child, "value");
                        break;
                    case "locked":
                        locked = Xml.GetBoolAttribute(child, "value");
                        break;
                    default:
                        break;
                }
            }
        }

        public override string ToString()
        {
            return $"{filename}: {fileType}: {contents}: {installPartition}{(overlay ? ", overlay" : "")}{(disabled ? ", disabled" : "")}{(persistent ? ", persistent" : "")}{(loadCompletely ? ", loadCompletely" : "")}{(locked ? ", locked" : "")}";
        }
    }

    public class DlcContentChangeSet
    {
        public string changeSetName { get; set; }
        public List<string>? filesToInvalidate { get; set; }
        public List<string>? filesToDisable { get; set; }
        public List<string>? filesToEnable { get; set; }
        public List<string>? txdToLoad { get; set; }
        public List<string>? txdToUnload { get; set; }
        public List<string>? residentResources { get; set; }
        public List<string>? unregisterResources { get; set; }
        public List<DlcContentChangeSet>? mapChangeSetData { get; set; }
        public string associatedMap { get; set; }
        public bool requiresLoadingScreen { get; set; }
        public string loadingScreenContext { get; set; }
        public bool useCacheLoader { get; set; }
        public DlcContentChangeSetExecutionConditions executionConditions { get; set; }

        public DlcContentChangeSet(XElement element)
        {
            Load(element);
        }

        public DlcContentChangeSet(XmlNode node)
        {
            Load(node);
        }

        public void Load(XElement element)
        {
            foreach(var child in element.Elements())
            {
                switch (child.Name.LocalName)
                {
                    case "changeSetName":
                        changeSetName = child.Value;
                        break;
                    case "filesToInvalidate":
                        filesToInvalidate = GetChildStringArray(child);
                        break;
                    case "filesToDisable":
                        filesToDisable = GetChildStringArray(child);
                        break;
                    case "filesToEnable":
                        filesToEnable = GetChildStringArray(child);
                        break;
                    case "txdToLoad":
                        txdToLoad = GetChildStringArray(child);
                        break;
                    case "txdToUnload":
                        txdToUnload = GetChildStringArray(child);
                        break;
                    case "residentResources":
                        residentResources = GetChildStringArray(child);
                        break;
                    case "unregisterResources":
                        unregisterResources = GetChildStringArray(child);
                        break;
                    case "mapChangeSetData":
                        if (child.HasElements)
                        {
                            mapChangeSetData = new List<DlcContentChangeSet>();
                            foreach (XElement childElement in child.Elements())
                            {
                                mapChangeSetData.Add(new DlcContentChangeSet(childElement));
                            }
                        }
                        break;
                    case "associatedMap":
                        associatedMap = child.Value;
                        break;
                    case "requiresLoadingScreen":
                        requiresLoadingScreen = Xml.GetBoolAttribute(child, "value");
                        break;
                    case "loadingScreenContext":
                        loadingScreenContext = child.Value;
                        break;
                    case "useCacheLoader":
                        useCacheLoader = Xml.GetBoolAttribute(child, "value");
                        break;
                    case "executionConditions":
                        executionConditions = new DlcContentChangeSetExecutionConditions(child);
                        break;
                    default:
                        break;
                }
            }
        }

        public void Load(XmlNode node)
        {
            foreach (XmlNode child in node.ChildNodes)
            {
                switch (child.Name)
                {
                    case "changeSetName":
                        changeSetName = child.InnerText;
                        break;
                    case "filesToInvalidate":
                        filesToInvalidate = GetChildStringArray(child);
                        break;
                    case "filesToDisable":
                        filesToDisable = GetChildStringArray(child);
                        break;
                    case "filesToEnable":
                        filesToEnable = GetChildStringArray(child);
                        break;
                    case "txdToLoad":
                        txdToLoad = GetChildStringArray(child);
                        break;
                    case "txdToUnload":
                        txdToUnload = GetChildStringArray(child);
                        break;
                    case "residentResources":
                        residentResources = GetChildStringArray(child);
                        break;
                    case "unregisterResources":
                        unregisterResources = GetChildStringArray(child);
                        break;
                    case "mapChangeSetData":
                        if (child.HasChildNodes)
                        {
                            mapChangeSetData = new List<DlcContentChangeSet>();
                            foreach (XmlNode mapChangeSetDataItem in child.ChildNodes)
                            {
                                mapChangeSetData.Add(new DlcContentChangeSet(mapChangeSetDataItem));
                            }
                        }
                        break;
                    case "associatedMap":
                        associatedMap = child.InnerText;
                        break;
                    case "requiresLoadingScreen":
                        requiresLoadingScreen = Xml.GetBoolAttribute(child, "value");
                        break;
                    case "loadingScreenContext":
                        loadingScreenContext = child.InnerText;
                        break;
                    case "useCacheLoader":
                        useCacheLoader = Xml.GetBoolAttribute(child, "value");
                        break;
                    case "executionConditions":
                        executionConditions = new DlcContentChangeSetExecutionConditions(child);
                        break;
                    default:
                        break;
                }
            }
        }

        private List<string> GetChildStringArray(XmlNode node)
        {
            if (!node.HasChildNodes) return null;
            var result = new List<string>();
            foreach (XmlNode child in node.ChildNodes)
            {
                if (child.NodeType == XmlNodeType.Element)
                {
                    result.Add(child.InnerText);
                }
            }
            return result;
        }

        private List<string>? GetChildStringArray(XElement node)
        {
            if (!node.HasElements) return null;

            var result = new List<string>();
            foreach(XElement childNode in node.Elements())
            {
                result.Add(childNode.Value);
            }

            return result;
        }


        public override string ToString()
        {
            return (changeSetName != null) ? changeSetName : (associatedMap != null) ? associatedMap : null;
        }

    }

    public class DlcContentChangeSetExecutionConditions
    {
        public string activeChangesetConditions { get; set; }
        public string genericConditions { get; set; }

        public DlcContentChangeSetExecutionConditions(XmlNode node)
        {
            Load(node);
        }

        public DlcContentChangeSetExecutionConditions(XElement element)
        {
            Load(element);
        }

        public void Load(XElement element)
        {
            foreach (XElement child in element.Elements())
            {
                switch (child.Name.LocalName)
                {
                    case "activeChangesetConditions":
                        activeChangesetConditions = child.Value;
                        break;
                    case "genericConditions":
                        genericConditions = child.Value;
                        break;
                    default:
                        break;
                }
            }
        }

        public void Load(XmlNode node)
        {
            foreach (XmlNode child in node.ChildNodes)
            {
                if (child.NodeType != XmlNodeType.Element) continue;
                switch (child.Name)
                {
                    case "activeChangesetConditions":
                        activeChangesetConditions = child.InnerText;
                        break;
                    case "genericConditions":
                        genericConditions = child.InnerText;
                        break;
                    default:
                        break;
                }
            }
        }

        public override string ToString()
        {
            return (string.IsNullOrEmpty(activeChangesetConditions) ? "" : activeChangesetConditions + ", ") + (string.IsNullOrEmpty(genericConditions) ? "" : genericConditions);
        }
    }



    public class DlcExtraFolderMountFile
    {
        public List<DlcExtraFolderMount> FolderMounts { get; set; } = new List<DlcExtraFolderMount>();

        public void Load(XmlDocument doc)
        {
            var root = doc.DocumentElement;

            XmlNodeList mountitems = doc.SelectNodes("SExtraFolderMountData/FolderMounts/Item");
            FolderMounts.Clear();
            for (int i = 0; i < mountitems.Count; i++)
            {
                var mount = new DlcExtraFolderMount();
                mount.Init(mountitems[i]);
                FolderMounts.Add(mount);
            }

        }

        public override string ToString()
        {
            return "(" + FolderMounts.Count.ToString() + " FolderMounts)";
        }
    }

    public class DlcExtraFolderMount
    {
        public string type { get; set; }
        public string platform { get; set; }
        public string path { get; set; }
        public string mountAs { get; set; }

        public void Init(XmlNode node)
        {
            type = Xml.GetStringAttribute(node, "type");
            platform = Xml.GetStringAttribute(node, "platform");
            path = Xml.GetChildInnerText(node, "path");
            mountAs = Xml.GetChildInnerText(node, "mountAs");
        }

        public override string ToString()
        {
            return type + ": " + path + " - " + mountAs + ((platform != null) ? (" (" + platform + ")") : "");
        }
    }


    public class DlcExtraTitleUpdateFile
    {
        public List<DlcExtraTitleUpdateMount> Mounts { get; set; } = new List<DlcExtraTitleUpdateMount>();

        public void Load(XmlDocument doc)
        {
            var root = doc.DocumentElement;

            XmlNodeList mountitems = doc.SelectNodes("SExtraTitleUpdateData/Mounts/Item");
            Mounts.Clear();
            for (int i = 0; i < mountitems.Count; i++)
            {
                var mount = new DlcExtraTitleUpdateMount();
                mount.Init(mountitems[i]);
                Mounts.Add(mount);
            }

        }

        public override string ToString()
        {
            return "(" + Mounts.Count.ToString() + " Mounts)";
        }
    }

    public class DlcExtraTitleUpdateMount
    {
        public string type { get; set; }
        public string deviceName { get; set; }
        public string path { get; set; }

        public void Init(XmlNode node)
        {
            type = Xml.GetStringAttribute(node, "type");
            deviceName = Xml.GetChildInnerText(node, "deviceName");
            path = Xml.GetChildInnerText(node, "path");
        }

        public override string ToString()
        {
            return type + ": " + deviceName + " - " + path;
        }
    }



}
