using CodeWalker.GameFiles;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CodeWalker.ModManager
{
    public partial class ModManagerForm : Form
    {
        public SettingsFile Settings;

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
            void initStatus(string msg)
            {
                Invoke(new Action(() =>
                {
                    StatusLabel.Text = msg;
                }));
            }
            void initSuccess()
            {
                Invoke(new Action(() =>
                {
                    RefreshInstalledMods();
                    StatusLabel.Text = "Ready";
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

            var waitform = (Form)null;
            void showWaitForm(string msg)
            {
                if (waitform != null) return;
                waitform = new Form();
                waitform.Text = "Please wait...";
                waitform.Size = new Size(200, 100);
                waitform.StartPosition = FormStartPosition.CenterParent;
                waitform.FormBorderStyle = FormBorderStyle.FixedDialog;
                waitform.MinimizeBox = false;
                waitform.MaximizeBox = false;
                waitform.ShowInTaskbar = false;
                var lbl = new Label();
                lbl.Text = msg;
                lbl.AutoSize = true;
                lbl.Location = new Point(10, 10);
                waitform.Controls.Add(lbl);
                BeginInvoke(new Action(() =>
                {
                    waitform.ShowDialog(this);
                }));
            }
            void hideWaitForm()
            {
                if (waitform == null) return;
                Invoke(new Action(() =>
                {
                    waitform.Close();
                    waitform = null;
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
                    var msg2 = $"For game: {(Settings.IsGen9 ? "GTAV Enhanced" : "GTAV Legacy")}";
                    initFail($"{msg1}\n{msg2}", "Invalid game folder");
                    return;
                }

                Invoke(new Action(UpdateGameButtonText));

                try //scan the game exe for the AES key if necessary, exit if unable to find it
                {
                    initStatus("Scanning game exe...");
                    showWaitForm("Scanning game exe...");
                    GTA5Keys.LoadFromPath(Settings.GameFolder, Settings.IsGen9, Settings.AESKey);
                    hideWaitForm();
                }
                catch 
                {
                    hideWaitForm();
                }
                if (GTA5Keys.PC_AES_KEY == null)
                {
                    var msg1 = $"Game exe not valid:\n{Settings.GameExePath}";
                    var msg2 = $"For game: {(Settings.IsGen9 ? "GTAV Enhanced" : "GTAV Legacy")}";
                    Settings.Reset();
                    initFail($"{msg1}\n{msg2}", "Invalid game exe");
                    return;
                }
                if (string.IsNullOrEmpty(Settings.AESKey))
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
    }
}
