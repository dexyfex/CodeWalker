using CodeWalker.GameFiles;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CodeWalker.ModManager
{
    public partial class ModManagerForm : Form
    {
        public SettingsFile Settings;
        public List<Mod> Mods = new List<Mod>();
        public Mod SelectedMod = null;

        public ModManagerForm()
        {
            InitializeComponent();

            Settings = new SettingsFile();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            Initialise();
        }


        private void Initialise()
        {
            void initFail(string msg, string title)
            {
                Invoke(new Action(() =>
                {
                    var msgex = "CodeWalker Mod Manager will now exit.";
                    MessageBoxEx.Show(this, $"{msg}\n\n{msgex}", title);
                    Close();
                }));
            }
            void initSuccess()
            {
                Invoke(new Action(() =>
                {
                    RefreshInstalledMods();//this doesn't really need to be invoked
                }));
            }

            void showSelectFolderForm()
            {
                Invoke(new Action(() =>
                {
                    var sff = new SelectFolderForm(Settings);
                    sff.ShowDialog(this);
                }));
            }


            Task.Run(() =>
            {

                if (Settings.FileExists == false)
                {
                    var msg1 = $"Unable to load {Settings.FileName}.";
                    var msg2 = "Please make sure you have installed CodeWalker Mod Manager correctly.";
                    var msg3 = "The exe should be running from its installation folder, and not the zip file.";
                    initFail($"{msg1}\n{msg2}\n{msg3}", "Settings file not found");
                    return;
                }

                if (Settings.GameFolderOk == false)
                {
                    showSelectFolderForm();
                }
                if (Settings.GameFolderOk == false)//still not valid after user tried to select invalid folder
                {
                    var gf = string.IsNullOrEmpty(Settings.GameFolder) ? "(None selected)" : Settings.GameFolder;
                    var msg1 = $"Game folder not valid: {gf}";
                    var msg2 = $"For game: {Settings.GameTitle}";
                    initFail($"{msg1}\n{msg2}", "Invalid game folder");
                    return;
                }

                Invoke(new Action(UpdateGameButtonText));

                var scankey = string.IsNullOrEmpty(Settings.AESKey);
                if (scankey)
                {
                    UpdateStatus("Scanning game exe...");
                    ShowWaitForm("Scanning game exe...");
                }
                try //scan the game exe for the AES key if necessary, exit if unable to find it
                {
                    GTA5Keys.LoadFromPath(Settings.GameFolder, Settings.IsGen9, Settings.AESKey);
                }
                catch
                { }
                if (scankey)
                {
                    HideWaitForm();
                }
                if (GTA5Keys.PC_AES_KEY == null)
                {
                    var msg1 = $"Game exe not valid:\n{Settings.GameExePath}";
                    var msg2 = $"For game: {Settings.GameTitle}";
                    Settings.Reset();//have another go at selecting a game folder next time the app is launched
                    initFail($"{msg1}\n{msg2}", "Invalid game exe");
                    return;
                }
                if (scankey)
                {
                    Settings.AESKey = Convert.ToBase64String(GTA5Keys.PC_AES_KEY);
                    Settings.Save();
                }

                initSuccess();

            });

        }

        private void UpdateGameButtonText()
        {
            GameButton.Text = "Game: " + Settings.GameName;
        }

        private void RefreshInstalledMods()
        {
            //load installed mods list and populate the installed mods UI

            Task.Run(() =>
            {
                ShowWaitForm("Loading Mods...");
                try
                {
                    RefreshInstalledModsList(UpdateStatus);
                    UpdateStatus("Ready");
                }
                catch (Exception ex)
                {
                    UpdateStatus("Error loading mods: " + ex.ToString());
                }
                HideWaitForm();

                Invoke(new Action(() => RefreshInstalledModsUI()));
            });


        }
        private void RefreshInstalledModsList(Action<string> updateStatus)
        {
            //iterate folders in the local cache and create Mod objects for them
            Mods.Clear();
            var cachedir = GetModCacheDir();
            var moddirs = Directory.Exists(cachedir) ? Directory.GetDirectories(cachedir) : new string[0];
            foreach (var moddir in moddirs)
            {
                var name = Path.GetFileName(moddir);
                if (updateStatus != null)
                {
                    updateStatus($"Scanning {name}...");
                }
                var mod = Mod.Load(moddir);
                if (mod != null)
                {
                    Mods.Add(mod);
                }
            }
            SortModsList();
        }
        private void RefreshInstalledModsUI()
        {
            //iterate Mods list to create installed mods UI

            SelectMod(null);

            InstalledModsListView.Items.Clear();
            foreach (var mod in Mods)
            {
                if (string.IsNullOrEmpty(mod?.Name)) continue;
                var item = InstalledModsListView.Items.Add(mod.Name);
                item.SubItems.Add(mod.Status.ToString());
                item.Tag = mod;
                var c = Color.Orange;
                switch (mod.Status)
                {
                    case ModStatus.Ready: c = Color.LightGreen; break;
                }
                item.BackColor = c;
            }

        }
        
        private void SelectMod(Mod mod)
        {
            SelectedMod = mod;
            if (mod == null)
            {
                ModPanel.Visible = false;
                ModPanel.SendToBack();
                SplashPanel.Visible = true;
                SplashPanel.BringToFront();
                ModNameLabel.Text = "No Mod Selected";
                ModStatusLabel.Text = "Disabled";
                ModFilesListBox.Items.Clear();
                return;
            }

            SplashPanel.Visible = false;
            SplashPanel.SendToBack();
            ModPanel.Visible = true;
            ModPanel.BringToFront();

            ModNameLabel.Text = mod.Name;
            ModStatusLabel.Text = mod.TypeStatusString;
            ModFilesListBox.Items.Clear();
            foreach (var file in mod.Files)
            {
                ModFilesListBox.Items.Add(file.Name);
            }


        }

        private void InstallMods(string[] files)
        {
            if (files == null) return;
            if (files.Length == 0) return;

            //check files are valid mods
            //TODO: have a UI to select/verify mods from these files?
            var okfiles = new List<string>();
            var badfiles = new List<string>();
            foreach (var file in files)
            {
                var ok = Mod.CanInstallFile(file);
                if (ok)
                {
                    okfiles.Add(file);
                    continue;
                }
                if (Directory.Exists(file))
                {
                    try
                    {
                        var subfiles = Directory.GetFiles(file, "*", SearchOption.AllDirectories);
                        if (subfiles == null) continue;
                        if (subfiles.Length == 0) continue;
                        if (subfiles.Length > 100)
                        {
                            badfiles.Add($"Too many subfiles ({subfiles.Length}): {file}");
                            continue;
                        }
                        foreach (var subfile in subfiles)
                        {
                            ok = Mod.CanInstallFile(subfile);
                            if (ok)
                            {
                                okfiles.Add(subfile);
                            }
                            else
                            {
                                //TODO: add this subfile to badfiles? or just ignore
                            }
                        }
                        continue;//don't add to badfiles if got to here
                    }
                    catch
                    { }//will get added to badfiles from here
                }
                badfiles.Add(file);
            }
            if (badfiles.Count > 0)
            {
                var msg = "Unable to install one or more files:";
                var msgex = string.Join(Environment.NewLine, badfiles);
                MessageBoxEx.Show(this, $"{msg}\n\n{msgex}", "Error installing");
            }

            files = okfiles.ToArray();//filter to only the supported files
            if (files.Length == 0) return;


            var errors = new List<string>();
            void installComplete()
            {
                Invoke(new Action(() =>
                {
                    if (errors.Count > 0)
                    {
                        var msg = "One or more errors were encountered:";
                        var msgex = string.Join(Environment.NewLine, errors);
                        MessageBoxEx.Show(this, $"{msg}\n\n{msgex}", "Error installing");
                    }
                    RefreshInstalledModsUI();
                }));
            }

            Task.Run(() =>
            {
                ShowWaitForm("Installing mods...");
                foreach (var file in files)
                {
                    try
                    {
                        InstallMod(file);
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"{file}\n{ex.Message}");
                    }
                }
                SortModsList();
                HideWaitForm();
                installComplete();
            });

        }
        private void InstallMod(string file)
        {
            var mod = Mod.BeginInstall(file);
            if (mod == null) return;
            if (string.IsNullOrEmpty(mod.Name)) return;

            //check for existing mods with the same name -> remove existing one first if same source path?
            //or add/replace a file in a loose files mod with the same name?
            var exmod = FindMod(mod.Name);
            if (exmod != null)
            {
                //if (file.Equals(exmod.SourcePath, StringComparison.OrdinalIgnoreCase))
                //{ }
                throw new Exception($"Mod already installed: {mod.Name}\nPlease uninstall first if you want to update it.");
            }

            mod.LoadOrder = GetNextLoadOrder();//set the load order to last by default, CompleteInstall could override

            var dir = EnsureModCacheDir(mod.Name);
            mod.CompleteInstall(dir);

            Mods.Add(mod);

        }

        private void UninstallMod(Mod mod)
        {
            if (mod == null) return;

            var msg = $"Are you sure you want to uninstall this mod?\n{mod.Name}";
            if (MessageBoxEx.Show(this, msg, "Uninstall Mod", MessageBoxButtons.YesNo) != DialogResult.Yes) return;

            SelectMod(null);
            var localdir = mod.LocalPath;
            if (Directory.Exists(localdir) == false) return;
            try
            {
                Directory.Delete(localdir, true);//apparently this might have issues?
            }
            catch (Exception ex)
            {
                MessageBoxEx.Show(this, "Unable to delete:\n" + ex.ToString());
                return;
            }

            RefreshInstalledMods();

        }

        private void SortModsList()
        {
            Mods.Sort((a, b) => a.LoadOrder.CompareTo(b.LoadOrder));
        }


        private string ModCacheDirName = "ModCache";
        private string GetModCacheDir()
        {
            var path = Assembly.GetExecutingAssembly().Location;
            var dir = Path.GetDirectoryName(path);
            return Path.Combine(dir, ModCacheDirName, Settings.GameModCache);
        }
        private string GetModCacheDir(string modname)
        {
            var cachedir = GetModCacheDir();
            return Path.Combine(cachedir, modname);
        }
        private string EnsureModCacheDir(string modname)
        {
            var moddir = GetModCacheDir(modname);
            if (Directory.Exists(moddir) == false)
            {
                Directory.CreateDirectory(moddir);
            }
            return moddir;
        }

        private Mod FindMod(string modname)
        {
            if (string.IsNullOrEmpty(modname)) return null;
            foreach (var mod in Mods)
            {
                if (mod == null) continue;
                if (string.IsNullOrEmpty(mod.Name)) continue;
                if (mod.Name.Equals(modname, StringComparison.OrdinalIgnoreCase)) return mod;
            }
            return null;
        }
        private int GetNextLoadOrder()
        {
            if (Mods.Count == 0) return 0;
            var maxlo = 0;
            foreach (var mod in Mods)
            {
                if (mod == null) continue;
                if (mod.LoadOrder > maxlo) maxlo = mod.LoadOrder;
            }
            return maxlo + 1;
        }


        private class WaitForm : Form
        {
            public Label TextLabel;

            public WaitForm(string title, string msg)
            {
                Text = title;
                Size = new Size(200, 100);
                StartPosition = FormStartPosition.CenterParent;
                FormBorderStyle = FormBorderStyle.FixedDialog;
                ControlBox = false;
                MinimizeBox = false;
                MaximizeBox = false;
                ShowInTaskbar = false;
                TextLabel = new Label();
                TextLabel.Text = msg;
                TextLabel.AutoSize = true;
                TextLabel.Location = new Point(10, 10);
                Controls.Add(TextLabel);
            }

        }
        private WaitForm _WaitForm;
        private void ShowWaitForm(string msg)
        {
            BeginInvoke(new Action(() =>
            {
                if (_WaitForm != null) return;
                _WaitForm = new WaitForm("Please wait...", msg);
                _WaitForm.ShowDialog(this);
            }));
        }
        private void HideWaitForm()
        {
            Invoke(new Action(() =>
            {
                if (_WaitForm == null) return;
                _WaitForm.Close();
                _WaitForm = null;
            }));
        }


        private void UpdateStatus(string msg)
        {
            Invoke(new Action(() =>
            {
                StatusLabel.Text = msg;
            }));
        }




        private void GameButton_Click(object sender, EventArgs e)
        {
            var sff = new SelectFolderForm(Settings);
            if (sff.ShowDialog(this) == DialogResult.OK)
            {
                UpdateGameButtonText();
                RefreshInstalledMods();
            }
        }

        private void InstallModButton_Click(object sender, EventArgs e)
        {
            //OpenFileDialog.Filter = "All Files|*.*";
            if (OpenFileDialog.ShowDialog(this) == DialogResult.OK)
            {
                InstallMods(OpenFileDialog.FileNames);
            }
        }

        private void ModManagerForm_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = e.Data.GetData(DataFormats.FileDrop) as string[];
                if ((files == null) || (files.Length <= 0)) return;
                e.Effect = DragDropEffects.Copy;
            }
        }

        private void ModManagerForm_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = e.Data.GetData(DataFormats.FileDrop) as string[];
                if ((files == null) || (files.Length <= 0)) return;
                InstallMods(files);
            }

        }

        private void InstalledModsListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (InstalledModsListView.SelectedItems.Count == 1)
            {
                SelectMod(InstalledModsListView.SelectedItems[0].Tag as Mod);
            }
            else
            {
                SelectMod(null);
            }
        }

        private void UninstallModButton_Click(object sender, EventArgs e)
        {
            UninstallMod(SelectedMod);
        }
    }
}
