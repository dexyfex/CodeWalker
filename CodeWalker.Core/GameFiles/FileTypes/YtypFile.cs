using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeWalker.GameFiles
{
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class YtypFile : PackedFile
    {

        public RpfFileEntry RpfFileEntry { get; set; }
        public string FilePath { get; set; }
        public string Name { get; set; }

        public Meta Meta { get; set; }
        public PsoFile Pso { get; set; }
        public RbfFile Rbf { get; set; }


        public uint NameHash { get; set; }
        public string[] Strings { get; set; }


        public CMapTypes CMapTypes { get; set; }

        public Archetype[] AllArchetypes { get; set; }

        public MetaWrapper[] Extensions { get; set; }

        public CCompositeEntityType[] CompositeEntityTypes { get; set; }


        //fields used by the editor:
        public bool HasChanged { get; set; } = false;
        public List<string> SaveWarnings = null;



        public override string ToString()
        {
            return (RpfFileEntry != null) ? RpfFileEntry.Name : string.Empty;
        }

        public void Load(byte[] data, RpfFileEntry entry)
        {
            Name = entry.Name;
            RpfFileEntry = entry;
            RpfResourceFileEntry resentry = entry as RpfResourceFileEntry;
            if (resentry == null)
            {
                MemoryStream ms = new MemoryStream(data);
                if (RbfFile.IsRBF(ms))
                {
                    Rbf = new RbfFile();
                    Rbf.Load(ms);
                }
                else if (PsoFile.IsPSO(ms))
                {
                    Pso = new PsoFile();
                    Pso.Load(ms);
                    //PsoTypes.EnsurePsoTypes(Pso);
                }
                else
                {
                }
                return;
            }





            ResourceDataReader rd = new ResourceDataReader(resentry, data);

            Meta = rd.ReadBlock<Meta>();


            CMapTypes = MetaTypes.GetTypedData<CMapTypes>(Meta, MetaName.CMapTypes);


            List<Archetype> allarchs = new List<Archetype>();

            var ptrs = MetaTypes.GetPointerArray(Meta, CMapTypes.archetypes);
            if (ptrs != null)
            {
                for (int i = 0; i < ptrs.Length; i++)
                {
                    var ptr = ptrs[i];
                    var offset = ptr.Offset;
                    var block = Meta.GetBlock(ptr.BlockID);
                    if (block == null)
                    { continue; }
                    if ((offset < 0) || (block.Data == null) || (offset >= block.Data.Length))
                    { continue; }

                    Archetype a = null;
                    switch (block.StructureNameHash)
                    {
                        case MetaName.CBaseArchetypeDef:
                            var basearch = PsoTypes.ConvertDataRaw<CBaseArchetypeDef>(block.Data, offset);
                            a = new Archetype();
                            a.Init(this, ref basearch);
                            a.Extensions = MetaTypes.GetExtensions(Meta, basearch.extensions);
                            break;
                        case MetaName.CTimeArchetypeDef:
                            var timearch = PsoTypes.ConvertDataRaw<CTimeArchetypeDef>(block.Data, offset);
                            var ta = new TimeArchetype();
                            ta.Init(this, ref timearch);
                            ta.Extensions = MetaTypes.GetExtensions(Meta, timearch._BaseArchetypeDef.extensions);
                            a = ta;
                            break;
                        case MetaName.CMloArchetypeDef:
                            var mloarch = PsoTypes.ConvertDataRaw<CMloArchetypeDef>(block.Data, offset);
                            var ma = new MloArchetype();
                            ma.Init(this, ref mloarch);
                            ma.Extensions = MetaTypes.GetExtensions(Meta, mloarch._BaseArchetypeDef.extensions);

                            ma.LoadChildren(Meta);

                            a = ma;
                            break;
                        default:
                            continue;
                    }

                    if (a != null)
                    {
                        allarchs.Add(a);
                    }
                }
            }
            AllArchetypes = allarchs.ToArray();


            Extensions = MetaTypes.GetExtensions(Meta, CMapTypes.extensions);
            if (Extensions != null)
            { }


            //AudioEmitters = MetaTypes.GetTypedDataArray<CExtensionDefAudioEmitter>(Meta, MetaName.CExtensionDefAudioEmitter);
            //if (AudioEmitters != null)
            //{ }

            //CEntityDefs = MetaTypes.GetTypedDataArray<CEntityDef>(Meta, MetaName.CEntityDef);


            CompositeEntityTypes = MetaTypes.ConvertDataArray<CCompositeEntityType>(Meta, MetaName.CCompositeEntityType, CMapTypes.compositeEntityTypes);
            if (CompositeEntityTypes != null)
            { }

            NameHash = CMapTypes.name;
            if (NameHash == 0)
            {
                int ind = entry.NameLower.LastIndexOf('.');
                if (ind > 0)
                {
                    NameHash = JenkHash.GenHash(entry.NameLower.Substring(0, ind));
                }
                else
                {
                    NameHash = JenkHash.GenHash(entry.NameLower);
                }
            }

            Strings = MetaTypes.GetStrings(Meta);
            if (Strings != null)
            {
                foreach (string str in Strings)
                {
                    JenkIndex.Ensure(str); //just shove them in there
                }
            }


            //foreach (var block in Meta.DataBlocks)
            //{
            //    switch(block.StructureNameHash)
            //    {
            //        case MetaName.CMapTypes:
            //        case MetaName.CTimeArchetypeDef:
            //        case MetaName.CBaseArchetypeDef:
            //        case MetaName.CMloArchetypeDef:
            //        case MetaName.CMloTimeCycleModifier:
            //        case MetaName.CMloRoomDef:
            //        case MetaName.CMloPortalDef:
            //        case MetaName.CMloEntitySet:
            //        case MetaName.CEntityDef:
            //        case MetaName.CExtensionDefParticleEffect:
            //        case MetaName.CExtensionDefAudioCollisionSettings:
            //        case MetaName.CExtensionDefSpawnPoint:
            //        case MetaName.CExtensionDefSpawnPointOverride:
            //        case MetaName.CExtensionDefExplosionEffect:
            //        case MetaName.CExtensionDefAudioEmitter:
            //        case MetaName.CExtensionDefLadder:
            //        case MetaName.CExtensionDefBuoyancy:
            //        case MetaName.CExtensionDefExpression:
            //        case MetaName.CExtensionDefLightShaft:
            //        case MetaName.CExtensionDefLightEffect:
            //        case MetaName.CExtensionDefDoor:
            //        case MetaName.CExtensionDefWindDisturbance:
            //        case MetaName.CExtensionDefProcObject:
            //        case MetaName.CLightAttrDef:
            //        case MetaName.STRING:
            //        case MetaName.POINTER:
            //        case MetaName.UINT:
            //        case MetaName.VECTOR4:
            //            break;
            //        default:
            //            break;
            //    }
            //}




            //MetaTypes.ParseMetaData(Meta);






        }

        public void Load(byte[] data) //REFACTOR THIS WITH YMAP!!
        {
            //direct load from a raw, compressed ymap file (openIV-compatible format)

            RpfResourceFileEntry resentry = new RpfResourceFileEntry();

            //hopefully this format has an RSC7 header...
            uint rsc7 = BitConverter.ToUInt32(data, 0);
            if (rsc7 == 0x37435352) //RSC7 header present!
            {
                int version = BitConverter.ToInt32(data, 4);
                resentry.SystemFlags = BitConverter.ToUInt32(data, 8);
                resentry.GraphicsFlags = BitConverter.ToUInt32(data, 12);
                if (data.Length > 16)
                {
                    int newlen = data.Length - 16; //trim the header from the data passed to the next step.
                    byte[] newdata = new byte[newlen];
                    Buffer.BlockCopy(data, 16, newdata, 0, newlen);
                    data = newdata;
                }
                else
                {
                    data = null; //shouldn't happen... empty..
                }
            }
            else
            {
                //direct load from file without the rpf header..
                //assume it's in resource meta format
                resentry.SystemFlags = RpfResourceFileEntry.GetFlagsFromSize(data.Length, 0);
                resentry.GraphicsFlags = RpfResourceFileEntry.GetFlagsFromSize(0, 2); //graphics type 2 for ymap
            }

            var oldresentry = RpfFileEntry as RpfResourceFileEntry;
            if (oldresentry != null) //update the existing entry with the new one
            {
                oldresentry.SystemFlags = resentry.SystemFlags;
                oldresentry.GraphicsFlags = resentry.GraphicsFlags;
                resentry.Name = oldresentry.Name;
                resentry.NameHash = oldresentry.NameHash;
                resentry.NameLower = oldresentry.NameLower;
                resentry.ShortNameHash = oldresentry.ShortNameHash;
            }
            else
            {
                RpfFileEntry = resentry; //just stick it in there for later...
            }

            data = ResourceBuilder.Decompress(data);


            Load(data, resentry);

            //Loaded = true;
        }

    }






}
