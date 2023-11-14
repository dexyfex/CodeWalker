



using CodeWalker.Properties;
using System;
using System.Diagnostics;

namespace CodeWalker.GameFiles
{

    public static class GameFileCacheFactory
    {
        public static GameFileCache? _instance;

        public static GameFileCache Instance
        {
            get
            {
                if (_instance is null)
                {
                    var s = Settings.Default;
                    _instance = new GameFileCache(s.CacheSize, s.CacheTime, GTAFolder.CurrentGTAFolder, s.DLC, s.EnableMods, s.ExcludeFolders, Settings.Default.Key);
                    GTAFolder.OnGTAFolderChanged += _instance.SetGtaFolder;
                }
                return _instance;
            }
        }
        public static GameFileCache GetInstance()
        {
            return Instance;
        }

    }

}