using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CodeWalker.ModManager
{
    public partial class SelectFolderForm : Form
    {
        public SettingsFile Settings;
        public string SelectedFolder { get; set; }
        public bool IsGen9 { get; set; }

        public SelectFolderForm(SettingsFile settings)
        {
            InitializeComponent();
            Settings = settings;
            SelectedFolder = Settings.GameFolder;
            IsGen9 = Settings.IsGen9;
            DialogResult = DialogResult.Cancel;
        }

        public static bool IsGen9Folder(string folder)
        {
            return File.Exists(folder + @"\gta5_enhanced.exe");
        }

        public static bool ValidateGTAFolder(string folder, bool gen9, out string failReason)
        {
            failReason = "";

            if (string.IsNullOrWhiteSpace(folder))
            {
                failReason = "No folder specified";
                return false;
            }

            if (!Directory.Exists(folder))
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
                if (!File.Exists(folder + @"\gta5.exe"))
                {
                    failReason = $"GTA5.exe not found in folder \"{folder}\"";
                    return false;
                }
            }

            return true;
        }

        private string AutodetectGameFolder()
        {
            var baseKey32 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
            var steamPathValue = baseKey32.OpenSubKey(@"Software\Rockstar Games\GTAV")?.GetValue("InstallFolderSteam") as string;
            var retailPathValue = baseKey32.OpenSubKey(@"Software\Rockstar Games\Grand Theft Auto V")?.GetValue("InstallFolder") as string;
            var oivPathValue = Registry.CurrentUser.OpenSubKey(@"Software\NewTechnologyStudio\OpenIV.exe\BrowseForFolder")?.GetValue("game_path_Five_pc") as string;

            if (steamPathValue?.EndsWith("\\GTAV") == true)
            {
                steamPathValue = steamPathValue.Substring(0, steamPathValue.LastIndexOf("\\GTAV"));
            }

            if (ValidateGTAFolder(steamPathValue, false, out _))
            {
                return steamPathValue; //matches.Add("Steam", steamPathValue);
            }

            if (ValidateGTAFolder(retailPathValue, false, out _))
            {
                return retailPathValue; //matches.Add("Retail", retailPathValue);
            }

            if (ValidateGTAFolder(oivPathValue, false, out _))
            {
                return oivPathValue; //matches.Add("OpenIV", oivPathValue);
            }

            return string.Empty;
        }

        private void SelectFolderForm_Load(object sender, EventArgs e)
        {
            FolderTextBox.Text = SelectedFolder;
            LegacyRadioButton.Checked = !IsGen9;
            EnhancedRadioButton.Checked = IsGen9;
            if (string.IsNullOrEmpty(SelectedFolder))
            {
                FolderTextBox.Text = AutodetectGameFolder();
            }
        }

        private void FolderBrowseButton_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog.SelectedPath = FolderTextBox.Text;
            DialogResult res = FolderBrowserDialog.ShowDialogNew();
            if (res == DialogResult.OK)
            {
                var path = FolderBrowserDialog.SelectedPath;
                var gen9 = IsGen9Folder(path);
                FolderTextBox.Text = path;
                LegacyRadioButton.Checked = !gen9;
                EnhancedRadioButton.Checked = gen9;
            }
        }

        private void FolderTextBox_TextChanged(object sender, EventArgs e)
        {
            SelectedFolder = FolderTextBox.Text;
        }

        private void EnhancedRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            IsGen9 = EnhancedRadioButton.Checked;
        }

        private void CancelButt_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void OkButton_Click(object sender, EventArgs e)
        {
            if (!ValidateGTAFolder(SelectedFolder, IsGen9, out string failReason))
            {
                MessageBoxEx.Show(this, "The selected folder could not be used:\n\n" + failReason, "Invalid GTA Folder", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Settings.GameFolder = SelectedFolder;
            Settings.IsGen9 = IsGen9;
            Settings.Save();

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
