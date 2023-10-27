


using CodeWalker.Properties;
using System;
using System.Diagnostics;

namespace CodeWalker.GameFiles
{

    public static class GameFileCacheFactory
    {
        public static GameFileCache _instance = null;
        public static GameFileCache Create()
        {
            if (_instance == null)
            {
                var s = Settings.Default;
                GTA5Keys.LoadFromPath(GTAFolder.CurrentGTAFolder, Settings.Default.Key);
                _instance = new GameFileCache(s.CacheSize, s.CacheTime, GTAFolder.CurrentGTAFolder, s.DLC, s.EnableMods, s.ExcludeFolders);
                GTAFolder.OnGTAFolderChanged += _instance.SetGtaFolder;
            }
            return _instance;
        }

    }

}