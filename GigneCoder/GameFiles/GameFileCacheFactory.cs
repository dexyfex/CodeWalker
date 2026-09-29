


using GigneCoder.Properties;

namespace GigneCoder.GameFiles
{

    public static class GameFileCacheFactory
    {

        public static GameFileCache Create()
        {
            var s = Settings.Default;
            return new GameFileCache(s.CacheSize, s.CacheTime, GTAFolder.CurrentGTAFolder, GTAFolder.IsGen9, s.DLC, s.EnableMods, s.ExcludeFolders);
        }

    }

}