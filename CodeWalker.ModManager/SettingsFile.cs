using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CodeWalker.ModManager
{
    public class SettingsFile : SimpleKvpFile
    {
        public string GameFolder;
        public bool IsGen9;
        public string AESKey;

        public string GameName => GameFolderOk ? IsGen9 ? "GTAV (Enhanced)" : "GTAV (Legacy)" : "(None selected)";
        public string GameTitle => IsGen9 ? "GTAV Enhanced" : "GTAV Legacy";
        public string GameExeName => IsGen9 ? "gta5_enhanced.exe" : "gta5.exe";
        public string GameExePath => $"{GameFolder}\\{GameExeName}";
        public string GameModCache => IsGen9 ? "GTAVEnhanced" : "GTAVLegacy";
        public bool GameFolderOk
        {
            get
            {
                if (Directory.Exists(GameFolder) == false) return false;
                if (File.Exists(GameExePath) == false) return false;
                return true;
            }
        }


        public SettingsFile()
        {
            FileName = "CodeWalker.ModManager.ini";
            OnlySaveIfFileExists = true;//only try and overwrite an existing settings file, as it is used to check if the exe is running in the correct directory!
            try
            {
                FilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), FileName);
                Load();
            }
            catch (Exception ex)
            {
                FileError = ex;
            }
        }

        public override void Load()
        {
            base.Load();
            GameFolder = GetItem("GameFolder");
            bool.TryParse(GetItem("IsGen9"), out IsGen9);
            AESKey = GetItem("AESKey");
        }
        public override void Save()
        {
            Items.Clear();
            SetItem("GameFolder", GameFolder);
            SetItem("IsGen9", IsGen9.ToString());
            SetItem("AESKey", AESKey);
            base.Save();
        }

        public void Reset()
        {
            GameFolder = null;
            IsGen9 = false;
            AESKey = null;
            Save();
        }

    }
}
