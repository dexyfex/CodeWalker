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
        private static string currentGTAFolder = Settings.Default.GTAFolder;

        public static event Action<string> OnGTAFolderChanged;
        public static string CurrentGTAFolder { get => currentGTAFolder; private set
            {
                if (currentGTAFolder == value) return;
                currentGTAFolder = value;
                OnGTAFolderChanged?.Invoke(currentGTAFolder);
            }
        }
        public static bool ValidateGTAFolder(string folder, out string failReason)
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

            if(!File.Exists(folder + @"\gta5.exe"))
            {
                failReason = $"GTA5.exe not found in folder \"{folder}\"";
                return false;
            }

            return true;
        }

        public static bool ValidateGTAFolder(string folder) => ValidateGTAFolder(folder, out string reason);

        public static bool IsCurrentGTAFolderValid() => ValidateGTAFolder(CurrentGTAFolder);

        public static bool UpdateGTAFolder(bool UseCurrentIfValid = false)
        {
            if(UseCurrentIfValid && IsCurrentGTAFolderValid())
            {
                return true;
            }

            string origFolder = CurrentGTAFolder;
            string folder = CurrentGTAFolder;
            SelectFolderForm f = new SelectFolderForm();

            string autoFolder = AutoDetectFolder(out string source);
            if (autoFolder != null && MessageBox.Show($"Auto-detected game folder \"{autoFolder}\" from {source}.\n\nContinue with auto-detected folder?", "Auto-detected game folder", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1) == DialogResult.Yes)
            {
                f.SelectedFolder = autoFolder;
            }

            f.ShowDialog();
            if(f.Result == DialogResult.OK && Directory.Exists(f.SelectedFolder))
            {
                folder = f.SelectedFolder;
            }

            string failReason;
            if(ValidateGTAFolder(folder, out failReason))
            {
                SetGTAFolder(folder);
                if(folder != origFolder)
                {
                    MessageBox.Show($"Successfully changed GTA Folder to \"{folder}\"", "Set GTA Folder", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                return true;
            } else
            {
                var tryAgain = MessageBox.Show($"Folder \"{folder}\" is not a valid GTA folder:\n\n{failReason}\n\nDo you want to try choosing a different folder?", "Unable to set GTA Folder", MessageBoxButtons.RetryCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
                if(tryAgain == DialogResult.Retry)
                {
                    return UpdateGTAFolder(false);
                } else
                {
                    return false;
                }
            }
        }

        public static bool SetGTAFolder(string folder)
        {
            if(ValidateGTAFolder(folder))
            {
                CurrentGTAFolder = folder;
                Settings.Default.GTAFolder = folder;
                Settings.Default.Save();
                return true;
            }

            return false;
        }

        public static string GetCurrentGTAFolderWithTrailingSlash() =>CurrentGTAFolder.EndsWith(@"\") ? CurrentGTAFolder : CurrentGTAFolder + @"\";

        public static bool AutoDetectFolder(out Dictionary<string, string> matches)
        {
            matches = new Dictionary<string, string>();

            if(ValidateGTAFolder(CurrentGTAFolder))
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

            if(ValidateGTAFolder(steamPathValue))
            {
                matches.Add("Steam", steamPathValue);
            }

            if(ValidateGTAFolder(retailPathValue))
            {
                matches.Add("Retail", retailPathValue);
            }

            if(ValidateGTAFolder(oivPathValue))
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
