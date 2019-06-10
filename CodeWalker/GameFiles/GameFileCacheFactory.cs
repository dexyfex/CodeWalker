


using CodeWalker.Properties;

namespace CodeWalker.GameFiles
{

    public static class GameFileCacheFactory
    {

        public static GameFileCache Create()
        {
            var s = Settings.Default;
            return new GameFileCache(s.CacheSize, s.CacheTime, GTAFolder.CurrentGTAFolder, s.DLC, s.EnableMods, s.ExcludeFolders);
        }

    }

}