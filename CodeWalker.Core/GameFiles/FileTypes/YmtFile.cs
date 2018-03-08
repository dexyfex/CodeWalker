using CodeWalker.World;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeWalker.GameFiles
{
    [TypeConverter(typeof(ExpandableObjectConverter))] public class YmtFile : GameFile, PackedFile
    {

        public Meta Meta { get; set; }
        public PsoFile Pso { get; set; }
        public RbfFile Rbf { get; set; }

        public YmtFileFormat FileFormat { get; set; } = YmtFileFormat.Unknown;
        public YmtFileContentType ContentType { get; set; } = YmtFileContentType.None;


        public Dictionary<string,string> CMapParentTxds { get; set; }

        public YmtScenarioPointManifest CScenarioPointManifest { get; set; }

        public MCScenarioPointRegion CScenarioPointRegion { get; set; }
        public ScenarioRegion ScenarioRegion { get; set; }




        //fields used by the editor:
        public bool HasChanged { get; set; } = false;
        public List<string> SaveWarnings = null;

        public YmtFile() : base(null, GameFileType.Ymt)
        {
        }
        public YmtFile(RpfFileEntry entry) : base(entry, GameFileType.Ymt)
        {
        }



        public void Load(byte[] data)
        {
            //direct load from a raw, compressed ymt resource file (openIV-compatible format)

            RpfFile.LoadResourceFile(this, data, 2);

            Loaded = true;
        }

        public void Load(byte[] data, RpfFileEntry entry)
        {
            RpfFileEntry = entry;
            Name = entry.Name;
            FilePath = Name;


            RpfResourceFileEntry resentry = entry as RpfResourceFileEntry;
            if (resentry != null)
            {
                ResourceDataReader rd = new ResourceDataReader(resentry, data);

                Meta = rd.ReadBlock<Meta>();

                var rootblock = Meta.GetRootBlock();
                if (rootblock != null)
                {
                    if (rootblock.StructureNameHash == MetaName.CScenarioPointRegion)
                    {
                        LoadScenarioPointRegion(Meta, rootblock);
                    }
                }

                Loaded = true;
                return;
            }


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
            if (PsoFile.IsPSO(ms))
            {
                Pso = new PsoFile();
                Pso.Load(ms);

                //PsoTypes.EnsurePsoTypes(Pso);

                var root = PsoTypes.GetRootEntry(Pso);
                if (root != null)
                {
                    if (root.NameHash == MetaName.CScenarioPointManifest)
                    {
                        LoadScenarioPointManifest(Pso);
                    }
                }


                Loaded = true;
                return;
            }
            else
            {

            }




        }


        private void LoadMapParentTxds(RbfStructure rbfstruct)
        {
            FileFormat = YmtFileFormat.RBF;
            ContentType = YmtFileContentType.MapParentTxds;

            CMapParentTxds = new Dictionary<string, string>();
            //StringBuilder sblist = new StringBuilder();
            foreach(var child in rbfstruct.Children)
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
                            foreach(var item in txdrelstruct.Children)
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
                            if((!string.IsNullOrEmpty(parentstr)) && (!string.IsNullOrEmpty(childstr)))
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

        private void LoadScenarioPointManifest(PsoFile pso)
        {
            FileFormat = YmtFileFormat.PSO;
            ContentType = YmtFileContentType.ScenarioPointManifest;

            CScenarioPointManifest = new YmtScenarioPointManifest();
            CScenarioPointManifest.Load(pso);

        }

        private void LoadScenarioPointRegion(Meta meta, MetaDataBlock rootblock)
        {
            FileFormat = YmtFileFormat.RSC;
            ContentType = YmtFileContentType.ScenarioPointRegion;

            var cdata = MetaTypes.ConvertData<CScenarioPointRegion>(rootblock.Data);

            CScenarioPointRegion = new MCScenarioPointRegion();
            CScenarioPointRegion.Ymt = this;
            CScenarioPointRegion.Load(meta, cdata);


            ScenarioRegion = new ScenarioRegion();
            ScenarioRegion.Load(this);

            //string stypes = MetaTypes.GetTypesInitString(meta);
            //if (!string.IsNullOrEmpty(stypes))
            //{ }
        }




        public byte[] Save()
        {

            switch (ContentType)
            {
                case YmtFileContentType.MapParentTxds: return SaveMapParentTxds();
                case YmtFileContentType.ScenarioPointManifest: return SaveScenarioPointManifest();
                case YmtFileContentType.ScenarioPointRegion: return SaveScenarioPointRegion();
            }

            return null;
        }


        private byte[] SaveMapParentTxds()
        {
            return null;
        }

        private byte[] SaveScenarioPointManifest()
        {
            return null;
        }

        private byte[] SaveScenarioPointRegion()
        {
            if (ScenarioRegion != null)
            {
                return ScenarioRegion.Save();
            }
            return null;
        }








        private void LogSaveWarning(string w)
        {
            if (SaveWarnings == null) SaveWarnings = new List<string>();
            SaveWarnings.Add(w);
        }






        public override string ToString()
        {
            return RpfFileEntry.ToString();
        }
    }


    public enum YmtFileFormat
    {
        Unknown = 0,
        RSC = 1,
        PSO = 2,
        RBF = 3,
    }
    public enum YmtFileContentType
    {
        None = 0,
        MapParentTxds = 1,
        ScenarioPointManifest = 2,
        ScenarioPointRegion = 3,
    }



    [TypeConverter(typeof(ExpandableObjectConverter))] public class YmtScenarioPointManifest
    {
        public CScenarioPointManifest _Data;
        public CScenarioPointManifest Data { get { return _Data; } set { _Data = value; } }

        public CScenarioPointRegionDef[] RegionDefs { get; set; }
        public CScenarioPointGroup[] Groups { get; set; }
        public MetaHash[] InteriorNames { get; set; }


        public void Load(PsoFile pso)
        {
            Data = PsoTypes.GetRootItem<CScenarioPointManifest>(pso);
            RegionDefs = PsoTypes.ConvertDataArray<CScenarioPointRegionDef>(pso, _Data.RegionDefs);
            Groups = PsoTypes.ConvertDataArray<CScenarioPointGroup>(pso, _Data.Groups);
            InteriorNames = PsoTypes.GetHashArray(pso, _Data.InteriorNames);
        }

    }


}
