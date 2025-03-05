using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;
using CodeWalker.Properties;
using Microsoft.Win32;
using CodeWalker.GameFiles;
using CodeWalker.Utils;

namespace CodeWalker
{
    public static class GTAFolder
    {
        public static string CurrentGTAFolder { get; private set; } = Settings.Default.GTAFolder;
        public static bool IsGen9 { get; private set; } = Settings.Default.GTAGen9;

        public static bool IsGen9Folder(string folder)
        {
            return File.Exists(folder + @"\gta5_enhanced.exe");
        }

        public static bool ValidateGTAFolder(string folder, bool gen9, out string failReason)
        {
            failReason = "";

            if(string.IsNullOrWhiteSpace(folder))
            {
                failReason = "No folder specified";
                return false;
            }

            if(!Directory.Exists(folder))
            {
                failReason = $"Folder \"{folder}\" does not exist";
                return false;
            }

            if (gen9)
            {
                if (!File.Exists(folder + @"\gta5_enhanced.exe"))
                {
                    failReason = $"GTA5_Enhanced.exe not found in folder \"{folder}\"";
                    return false;
                }
            }
            else
            {
                if(!File.Exists(folder + @"\gta5.exe"))
                {
                    failReason = $"GTA5.exe not found in folder \"{folder}\"";
                    return false;
                }
            }

            return true;
        }

        public static bool ValidateGTAFolder(string folder, bool gen9) => ValidateGTAFolder(folder, gen9, out string reason);

        public static bool IsCurrentGTAFolderValid() => ValidateGTAFolder(CurrentGTAFolder, IsGen9);

        public static bool UpdateGTAFolder(bool useCurrentIfValid = false, bool autoDetect = true)
        {
            if (useCurrentIfValid && IsCurrentGTAFolderValid())
            {
                return true;
            }

            var gen9 = IsGen9;
            string origFolder = CurrentGTAFolder;
            string folder = CurrentGTAFolder;
            SelectFolderForm f = new SelectFolderForm();

            if (autoDetect)
            {
                string autoFolder = AutoDetectFolder(out string source);
                if (autoFolder != null && MessageBox.Show($"Auto-detected game folder \"{autoFolder}\" from {source}.\n\nContinue with auto-detected folder?", "Auto-detected game folder", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1) == DialogResult.Yes)
                {
                    f.SelectedFolder = autoFolder;
                    f.IsGen9 = IsGen9Folder(autoFolder);
                }
            }

            f.ShowDialog();
            if (f.Result == DialogResult.Cancel)
            {
                return false;
            }
            if (f.Result == DialogResult.OK && Directory.Exists(f.SelectedFolder))
            {
                folder = f.SelectedFolder;
                gen9 = f.IsGen9;
            }

            string failReason;
            if (ValidateGTAFolder(folder, gen9, out failReason))
            {
                SetGTAFolder(folder, gen9);
                if (folder != origFolder)
                {
                    MessageBox.Show($"Successfully changed GTA Folder to \"{folder}\"", "Set GTA Folder", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                return true;
            }
            else
            {
                var tryAgain = MessageBox.Show($"Folder \"{folder}\" is not a valid GTA folder:\n\n{failReason}\n\nDo you want to try choosing a different folder?", "Unable to set GTA Folder", MessageBoxButtons.RetryCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
                if (tryAgain == DialogResult.Retry)
                {
                    return UpdateGTAFolder(false);
                }
                else
                {
                    return false;
                }
            }
        }

        public static bool SetGTAFolder(string folder, bool gen9)
        {
            if(ValidateGTAFolder(folder, gen9))
            {
                CurrentGTAFolder = folder;
                IsGen9 = gen9;
                Settings.Default.GTAFolder = folder;
                Settings.Default.GTAGen9 = gen9;
                Settings.Default.Save();
                return true;
            }

            return false;
        }

        public static string GetCurrentGTAFolderWithTrailingSlash() =>CurrentGTAFolder.EndsWith(@"\") ? CurrentGTAFolder : CurrentGTAFolder + @"\";

        public static bool AutoDetectFolder(out Dictionary<string, string> matches)
        {
            matches = new Dictionary<string, string>();

            if(ValidateGTAFolder(CurrentGTAFolder, IsGen9))
            {
                matches.Add("Current CodeWalker Folder", CurrentGTAFolder);
            }

            RegistryKey baseKey32 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
            string steamPathValue = baseKey32.OpenSubKey(@"Software\Rockstar Games\GTAV")?.GetValue("InstallFolderSteam") as string;
            string retailPathValue = baseKey32.OpenSubKey(@"Software\Rockstar Games\Grand Theft Auto V")?.GetValue("InstallFolder") as string;
            string oivPathValue = Registry.CurrentUser.OpenSubKey(@"Software\NewTechnologyStudio\OpenIV.exe\BrowseForFolder")?.GetValue("game_path_Five_pc") as string;

            if(steamPathValue?.EndsWith("\\GTAV") == true)
            {
                steamPathValue = steamPathValue.Substring(0, steamPathValue.LastIndexOf("\\GTAV"));
            }

            if(ValidateGTAFolder(steamPathValue, false))
            {
                matches.Add("Steam", steamPathValue);
            }

            if(ValidateGTAFolder(retailPathValue, false))
            {
                matches.Add("Retail", retailPathValue);
            }

            if(ValidateGTAFolder(oivPathValue, false))
            {
                matches.Add("OpenIV", oivPathValue);
            }

            return matches.Count > 0;
        }

        public static string AutoDetectFolder(out string source)
        {
            source = null;

            if(AutoDetectFolder(out Dictionary<string, string> matches))
            {
                var match = matches.First();
                source = match.Key;
                return match.Value;
            }

            return null;
        }

        public static string AutoDetectFolder() => AutoDetectFolder(out string _);

        public static void UpdateSettings()
        {
            if (string.IsNullOrEmpty(Settings.Default.Key) && (GTA5Keys.PC_AES_KEY != null))
            {
                Settings.Default.Key = Convert.ToBase64String(GTA5Keys.PC_AES_KEY);
                Settings.Default.Save();
            }
        }
    }
}
