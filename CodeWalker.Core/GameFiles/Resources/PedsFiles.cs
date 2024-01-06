using CodeWalker.GameFiles;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeWalker.Core.GameFiles.Resources
{
    public class CustomTypeConverter : ExpandableObjectConverter
    {
        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext? context, object value, Attribute[]? attributes)
        {
            Console.WriteLine($"{context}: {value}");

            var properties = TypeDescriptor.GetProperties(value, attributes);

            Console.WriteLine(properties);

            return properties;
        }
    }

    public class DictionaryTypeConverter<TKey, TValue> : ExpandableObjectConverter
    {
        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext? context, object value, Attribute[]? attributes)
        {
            Console.WriteLine($"{context}: {value}");

            var properties = TypeDescriptor.GetProperties(value, attributes);

            if (value is IDictionary<TKey, TValue> dict)
            {
                foreach (var v in dict)
                {
                    properties.Add(TypeDescriptor.GetDefaultProperty(v.Value));
                }
            }


            Console.WriteLine(properties);

            return properties;
        }
    }


    public class DictionaryConvert<TKey, TValue> : TypeConverter
    {
        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext? context, object value, Attribute[]? attributes)
        {
            Console.WriteLine($"{context}: {value}");

            var properties = TypeDescriptor.GetProperties(value, attributes);

            Console.WriteLine(properties);

            return properties;
        }
    }


    [TypeConverter(typeof(CustomTypeConverter))]
    public class PedsDlcFiles
    {
        public MetaHash DlcName { get; set; }
        public PedsDlcFiles(MetaHash dlcName)
        {
            DlcName = dlcName;
        }
        [Browsable(true)]
        public PedFile PedFile { get; set; }
        [Browsable(true)]
        public ConcurrentDictionary<MetaHash, RpfFileEntry> Drawables { get; set; } = new ConcurrentDictionary<MetaHash, RpfFileEntry>();
        [Browsable(true)]
        public ConcurrentDictionary<MetaHash, RpfFileEntry> TextureDicts { get; set; } = new ConcurrentDictionary<MetaHash, RpfFileEntry>();
        [Browsable(true)]
        public ConcurrentDictionary<MetaHash, RpfFileEntry> ClothDicts { get; set; } = new ConcurrentDictionary<MetaHash, RpfFileEntry>();

        public int Index
        {
            get {
                if (!GameFileCache.Instance.DlcNameLookup.TryGetValue(DlcName, out var dlcName))
                {
                    return -1;
                }

                return GameFileCache.Instance.DlcNameList.FindIndex(0, (value) => value.Equals(dlcName, StringComparison.OrdinalIgnoreCase));
            }
        }
    }

    [TypeConverter(typeof(CustomTypeConverter))]
    public class PedsFiles
    {
        [Browsable(true)]
        [TypeConverter(typeof(DictionaryTypeConverter<MetaHash, PedsDlcFiles>))]
        public ConcurrentDictionary<MetaHash, PedsDlcFiles> Dlcs { get; set; } = new ConcurrentDictionary<MetaHash, PedsDlcFiles>();
        [Browsable(true)]
        [TypeConverter(typeof(CollectionConverter))]
        public ICollection<PedFile> Ymts { get; set; } = new HashSet<PedFile>(GameFileByPathComparer.Instance);

        public string kaas = "Kaas";

        public PedsDlcFiles GetPedsDlcFiles(PedFile pedFile)
        {
            var pedsDlcFiles = GetPedsDlcFiles(pedFile.DlcName);
            pedsDlcFiles.PedFile = pedFile;
            return pedsDlcFiles;
        }

        public PedsDlcFiles GetPedsDlcFiles(MetaHash dlcName)
        {
            if (!Dlcs.TryGetValue(dlcName, out var pedsFiles))
            {
                _ = Dlcs.TryAdd(dlcName, new PedsDlcFiles(dlcName));

                pedsFiles = Dlcs[dlcName];
            }

            return pedsFiles;
        }

        public bool TryGetPedsDlcFiles(PedFile pedFile, out PedsDlcFiles pedsDlcFiles)
        {
            return TryGetPedsDlcFiles(pedFile.DlcName, out pedsDlcFiles);
        }

        public bool TryGetPedsDlcFiles(MetaHash dlcName, out PedsDlcFiles pedsDlcFiles)
        {
            return Dlcs.TryGetValue(dlcName, out pedsDlcFiles);
        }

        public void AddDrawable(PedFile pedFile, RpfFileEntry entry)
        {
            var pedsFiles = GetPedsDlcFiles(pedFile);

            pedsFiles.Drawables.TryAdd(entry.ShortNameHash, entry);
        }

        public void AddTextureDict(PedFile pedFile, RpfFileEntry entry)
        {
            var pedsFiles = GetPedsDlcFiles(pedFile);

            pedsFiles.TextureDicts.TryAdd(entry.ShortNameHash, entry);
        }

        public void AddClothsDict(PedFile pedFile, RpfFileEntry entry)
        {
            var pedsFiles = GetPedsDlcFiles(pedFile);

            pedsFiles.ClothDicts.TryAdd(entry.ShortNameHash, entry);
        }
    }
}
