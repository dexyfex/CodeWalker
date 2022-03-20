using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeWalker.GameFiles
{
    public abstract class GameFile : Cacheable<GameFileCacheKey>
    {
        public volatile bool Loaded = false;
        public volatile bool LoadQueued = false;
        public RpfFileEntry RpfFileEntry { get; set; }
        public string Name { get; set; }
        public string FilePath { get; set; } //used by the project form.
        public GameFileType Type { get; set; }



        public GameFile(RpfFileEntry entry, GameFileType type)
        {
            RpfFileEntry = entry;
            Type = type;
            MemoryUsage = (entry != null) ? entry.GetFileSize() : 0;
            if (entry is RpfResourceFileEntry)
            {
                var resent = entry as RpfResourceFileEntry;
                var newuse = resent.SystemSize + resent.GraphicsSize;
                MemoryUsage = newuse;
            }
            else if (entry is RpfBinaryFileEntry)
            {
                var binent = entry as RpfBinaryFileEntry;
                var newuse = binent.FileUncompressedSize;
                if (newuse > MemoryUsage)
                {
                    MemoryUsage = newuse;
                }
            }
            else
            {
            }
        }

        public override string ToString()
        {
            return (string.IsNullOrEmpty(Name)) ? JenkIndex.GetString(Key.Hash) : Name;
        }


    }


    public enum GameFileType : int
    {
        Ydd = 0,
        Ydr = 1,
        Yft = 2,
        Ymap = 3,
        Ymf = 4,
        Ymt = 5,
        Ytd = 6,
        Ytyp = 7,
        Ybn = 8,
        Ycd = 9,
        Ypt = 10,
        Ynd = 11,
        Ynv = 12,
        Rel = 13,
        Ywr = 14,
        Yvr = 15,
        Gtxd = 16,
        Vehicles = 17,
        CarCols = 18,
        CarModCols = 19,
        CarVariations = 20,
        VehicleLayouts = 21,
        Peds = 22,
        Ped = 23,
        Yed = 24,
        Yld = 25,
        Yfd = 26,
        Heightmap = 27,
        Watermap = 28,
        Mrf = 29,
        DistantLights = 30,
        Ypdb = 31,
    }





    public struct GameFileCacheKey
    {
        public uint Hash { get; set; }
        public GameFileType Type { get; set; }

        public GameFileCacheKey(uint hash, GameFileType type)
        {
            Hash = hash;
            Type = type;
        }
    }



}
