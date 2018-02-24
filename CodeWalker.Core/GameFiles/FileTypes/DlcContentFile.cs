using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace CodeWalker.GameFiles
{
    public class DlcContentFile
    {

        public List<DlcContentDataFile> dataFiles { get; set; } = new List<DlcContentDataFile>();
        public List<DlcContentChangeSet> contentChangeSets { get; set; } = new List<DlcContentChangeSet>();

        public RpfFile DlcFile { get; set; } //used by GameFileCache
        public Dictionary<string, DlcExtraFolderMountFile> ExtraMounts { get; set; } = new Dictionary<string, DlcExtraFolderMountFile>();
        public Dictionary<string, DlcContentDataFile> RpfDataFiles { get; set; } = new Dictionary<string, DlcContentDataFile>();

        public DlcExtraTitleUpdateFile ExtraTitleUpdates { get; set; }

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
                        foreach (XmlNode disabledFile in node.ChildNodes)
                        { } //nothing to see here..
                        break;
                    case "includedXmlFiles":
                        foreach (XmlNode includedXmlFile in node.ChildNodes)
                        { } //nothing to see here..
                        break;
                    case "includedDataFiles":
                        foreach (XmlNode includedDataFile in node.ChildNodes)
                        { } //nothing to see here..
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
                        foreach (XmlNode patchFile in node.ChildNodes)
                        { } //nothing to see here..
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
                string dfn = GameFileCache.GetDlcPlatformPath(datafile.filename).ToLowerInvariant();
                if (datafile.fileType == "EXTRA_FOLDER_MOUNT_DATA")
                {
                    string efmdxmlpath = datafile.filename.Replace(setupfile.deviceName + ":", DlcFile.Path).Replace('/', '\\');
                    efmdxmlpath = gfc.GetDlcPatchedPath(efmdxmlpath);
                    XmlDocument efmdxml = rpfman.GetFileXml(efmdxmlpath);

                    DlcExtraFolderMountFile efmf = new DlcExtraFolderMountFile();
                    efmf.Load(efmdxml);

                    ExtraMounts[dfn] = efmf;
                }
                if (datafile.fileType == "EXTRA_TITLE_UPDATE_DATA")
                {
                    string etudxmlpath = datafile.filename.Replace(setupfile.deviceName + ":", DlcFile.Path).Replace('/', '\\');
                    etudxmlpath = gfc.GetDlcPatchedPath(etudxmlpath);
                    XmlDocument etudxml = rpfman.GetFileXml(etudxmlpath);

                    DlcExtraTitleUpdateFile etuf = new DlcExtraTitleUpdateFile();
                    etuf.Load(etudxml);

                    ExtraTitleUpdates = etuf;
                }
                if (datafile.fileType == "RPF_FILE")
                {
                    RpfDataFiles[dfn] = datafile;
                }
            }

        }

        public override string ToString()
        {
            return dataFiles.Count.ToString() + " dataFiles, " + contentChangeSets.Count.ToString() + " contentChangeSets";
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
            return filename + ": " + fileType + ": " + contents + ": " + installPartition +
                (overlay ? ", overlay" : "") + 
                (disabled ? ", disabled" : "") +
                (persistent ? ", persistent" : "") +
                (loadCompletely ? ", loadCompletely" : "") +
                (locked ? ", locked" : "");
        }
    }

    public class DlcContentChangeSet
    {
        public string changeSetName { get; set; }
        public List<string> filesToInvalidate { get; set; }
        public List<string> filesToDisable { get; set; }
        public List<string> filesToEnable { get; set; }
        public List<string> txdToLoad { get; set; }
        public List<string> txdToUnload { get; set; }
        public List<string> residentResources { get; set; }
        public List<string> unregisterResources { get; set; }
        public List<DlcContentChangeSet> mapChangeSetData { get; set; }
        public string associatedMap { get; set; }
        public bool requiresLoadingScreen { get; set; }
        public string loadingScreenContext { get; set; }
        public bool useCacheLoader { get; set; }
        public DlcContentChangeSetExecutionConditions executionConditions { get; set; }

        public DlcContentChangeSet(XmlNode node)
        {
            Load(node);
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
                        if (filesToInvalidate != null)
                        { }
                        break;
                    case "filesToDisable":
                        filesToDisable = GetChildStringArray(child);
                        if (filesToDisable != null)
                        { }
                        break;
                    case "filesToEnable":
                        filesToEnable = GetChildStringArray(child);
                        if (filesToEnable != null)
                        { }
                        break;
                    case "txdToLoad":
                        txdToLoad = GetChildStringArray(child);
                        if (txdToLoad != null)
                        { }
                        break;
                    case "txdToUnload":
                        txdToUnload = GetChildStringArray(child);
                        if (txdToUnload != null)
                        { }
                        break;
                    case "residentResources":
                        residentResources = GetChildStringArray(child);
                        if (residentResources != null)
                        { }
                        break;
                    case "unregisterResources":
                        unregisterResources = GetChildStringArray(child);
                        if (unregisterResources != null)
                        { }
                        break;
                    case "mapChangeSetData":
                        mapChangeSetData = new List<DlcContentChangeSet>();
                        foreach (XmlNode mapChangeSetDataItem in child.ChildNodes)
                        {
                            mapChangeSetData.Add(new DlcContentChangeSet(mapChangeSetDataItem));
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
