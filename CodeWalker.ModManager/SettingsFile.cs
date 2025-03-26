using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CodeWalker.ModManager
{
    public class SettingsFile
    {
        public string FileName = "CodeWalker.ModManager.ini";
        public string FilePath;
        public bool FileExists;
        public Exception FileError;

        public string GameFolder;
        public bool IsGen9;
        public string AESKey;

        public string GameName => GameFolderOk ? IsGen9 ? "GTAV (Enhanced)" : "GTAV (Legacy)" : "(None selected)";
        public string GameExeName => IsGen9 ? "gta5_enhanced.exe" : "gta5.exe";
        public string GameExePath => $"{GameFolder}\\{GameExeName}";
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
            var path = Assembly.GetExecutingAssembly().Location;
            var dir = Path.GetDirectoryName(path);
            FilePath = Path.Combine(dir, FileName);
            FileExists = File.Exists(FilePath);
            Load();
        }

        public void Load()
        {
            if (FileExists == false) return;
            try
            {
                var lines = File.ReadAllLines(FilePath);
                if (lines == null) return;
                foreach (var line in lines)
                {
                    var tline = line?.Trim();
                    if (string.IsNullOrEmpty(tline)) continue;
                    var spi = tline.IndexOf(' ');
                    if (spi < 1) continue;
                    if (spi >= (tline.Length - 1)) continue;
                    var key = tline.Substring(0, spi).Trim();
                    var val = tline.Substring(spi + 1).Trim();
                    if (string.IsNullOrEmpty(key)) continue;
                    if (string.IsNullOrEmpty(val)) continue;
                    switch (key)
                    {
                        case "GameFolder": GameFolder = val; break;
                        case "IsGen9": bool.TryParse(val, out IsGen9); break;
                        case "AESKey": AESKey = val; break;
                    }
                }
            }
            catch (Exception ex)
            {
                FileError = ex;
                FileExists = false;
            }
        }

        public void Save()
        {
            if (FileExists == false) return;
            try
            {
                var sb = new StringBuilder();
                sb.AppendLine($"GameFolder {GameFolder}");
                sb.AppendLine($"IsGen9 {IsGen9}");
                sb.AppendLine($"AESKey {AESKey}");
                var str = sb.ToString();
                File.WriteAllText(FilePath, str);
            }
            catch { }
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
