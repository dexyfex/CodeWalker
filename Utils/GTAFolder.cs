using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;
using CodeWalker.Properties;

namespace CodeWalker
{
    public static class GTAFolder
    {
        public static string CurrentGTAFolder { get; private set; } = Settings.Default.GTAFolder;

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

    }
}
