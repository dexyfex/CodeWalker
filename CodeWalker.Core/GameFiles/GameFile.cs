          using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace CodeWalker.GameFiles
{

    // This would make more sense as a enum, but that would cause lots of bloat casting values
    public static class LoadState
    {
        public const int None = 0;
        public const int Loaded = 1 << 0;
        public const int LoadQueued = 1 << 1;
        public const int Disposed = 1 << 2;

        public static bool ToggleFlag(ref int currentValue, int flagToToggle, bool value)
        {
            if (value)
            {
                return (Interlocked.Or(ref currentValue, flagToToggle) & flagToToggle) == 0;
            }
            else
            {
                return (Interlocked.And(ref currentValue, ~flagToToggle) & flagToToggle) == flagToToggle;
            }
        }
    }

    public abstract class GameFile : Cacheable<GameFileCacheKey>, IDisposable
    {
        public byte LoadAttempts = 0;

        public int loadState = (int)LoadState.None;

        public bool LoadQueued { 
            get => (loadState & LoadState.LoadQueued) == LoadState.LoadQueued;
            set => SetLoadQueued(value);
        }

        public bool Loaded
        {
            get => (loadState & LoadState.Loaded) == LoadState.Loaded;
            set => SetLoaded(value);
        }

        public bool IsDisposed {
            get => (loadState & LoadState.Disposed) == LoadState.Disposed;
            set => SetDisposed(value);
        }

        public DateTime LastLoadTime = DateTime.MinValue;
        public RpfFileEntry? RpfFileEntry { get; set; }
        public string Name { get; set; }
        public string FilePath { get; set; } //used by the project form.
        public GameFileType Type { get; set; }



        public GameFile(RpfFileEntry? entry, GameFileType type)
        {
            RpfFileEntry = entry;
            Type = type;
            MemoryUsage = (entry != null) ? entry.GetFileSize() : 0;
            if (entry is RpfResourceFileEntry resent)
            {
                var newuse = resent.SystemSize + resent.GraphicsSize;
                MemoryUsage = newuse;
            }
            else if (entry is RpfBinaryFileEntry binent)
            {
                var newuse = binent.FileUncompressedSize;
                if (newuse > MemoryUsage)
                {
                    MemoryUsage = newuse;
                }
            }
        }

        public bool SetLoadQueued(bool value) => LoadState.ToggleFlag(ref loadState, LoadState.LoadQueued, value);

        public bool SetLoaded(bool value) => LoadState.ToggleFlag(ref loadState, LoadState.Loaded, value);

        public bool SetDisposed(bool value) => LoadState.ToggleFlag(ref loadState, LoadState.Disposed, value);

        public override string ToString() => string.IsNullOrEmpty(Name) ? JenkIndex.GetString(Key.Hash) : Name;

        public virtual void Dispose()
        {
            IsDisposed = true;
            GC.SuppressFinalize(this);
        }

        [DoesNotReturn]
        public static void ThrowFileIsNotAResourceException()
        {
            throw new Exception("File entry wasn't a resource! (is it binary data?)");
        }
    }

    public class GameFileByPathComparer : IEqualityComparer<GameFile>
    {
        public static readonly GameFileByPathComparer Instance = new GameFileByPathComparer();
        public bool Equals(GameFile? x, GameFile? y)
        {
            if (x is null && y is null)
                return true;
            if (x is null || y is null)
                return false;
            if (ReferenceEquals(x, y))
                return true;

            if (x.Type != y.Type)
                return false;

            if (x.RpfFileEntry is null && y.RpfFileEntry is null)
                return true;
            if (x.RpfFileEntry is null || y.RpfFileEntry is null)
                return false;

            return x.RpfFileEntry.Path.Equals(y.RpfFileEntry.Path, StringComparison.OrdinalIgnoreCase);
        }

        public int GetHashCode([DisallowNull] GameFile obj)
        {
            return HashCode.Combine(obj.RpfFileEntry?.Path.GetHashCode(StringComparison.OrdinalIgnoreCase) ?? 0, obj.Type);
        }
    }


    public enum GameFileType : byte
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
        PedShopMeta = 32,
        None = 33,
    }





    public readonly record struct GameFileCacheKey(uint Hash, GameFileType Type);
}
