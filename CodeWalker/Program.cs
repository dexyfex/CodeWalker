using CodeWalker.Core.Utils;
using CodeWalker.Forms;
using CodeWalker.GameFiles;
using CodeWalker.Properties;
using CodeWalker.Utils;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Shell;

namespace CodeWalker;
public static class Program
{
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main(string[] args)
    {
        ConsoleWindow.Hide();
        _ = Task.Run(() =>
        {
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            

            Application.ThreadException += (object sender, ThreadExceptionEventArgs e) =>
            {
                Console.WriteLine($"Unhandeled exception occured: {e.Exception}");
            };
        });
        LoadConfig();
        bool menumode = false;
        bool explorermode = false;
        bool projectmode = false;
        bool vehiclesmode = false;
        bool pedsmode = false;
        string path = null;
        if ((args != null) && (args.Length > 0))
        {
            foreach (string arg in args)
            {
                string argl = arg.ToLowerInvariant();
                if (argl == "menu")
                {
                    menumode = true;
                }
                if (argl == "explorer")
                {
                    explorermode = true;
                }
                if (argl == "project")
                {
                    projectmode = true;
                }
                if (argl == "vehicles")
                {
                    vehiclesmode = true;
                }
                if (argl == "peds")
                {
                    pedsmode = true;
                }
                try
                {
                    if (File.Exists(arg))
                    {
                        path = arg;
                    }
                } catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                    Console.WriteLine(ex);
                }
                    
            }
        }

        EnsureJumpList();

        //Application.SetHighDpiMode(HighDpiMode.SystemAware);
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);


        // Always check the GTA folder first thing
        if (!GTAFolder.UpdateGTAFolder(Properties.Settings.Default.RememberGTAFolder))
        {
            MessageBox.Show("Could not load CodeWalker because no valid GTA 5 folder was selected. CodeWalker will now exit.", "GTA 5 Folder Not Found", MessageBoxButtons.OK, MessageBoxIcon.Stop);
            return;
        }
#if !DEBUG
        try
        {
#endif
            if (menumode)
            {
                Application.Run(new MenuForm());
            }
            else if (explorermode)
            {
                if (!NamedPipe.TrySendMessageToOtherProcess("explorer"))
                {
                    var form = new ExploreForm();
                    var namedPipe = new NamedPipe(form);
                    namedPipe.StartServer();
                    Application.Run(form);
                }
            }
            else if (projectmode)
            {
                Application.Run(new Project.ProjectForm());
            }
            else if (vehiclesmode)
            {
                Application.Run(new VehicleForm());
            }
            else if (pedsmode)
            {
                if (!NamedPipe.TrySendMessageToOtherProcess("peds-mode"))
                {
                    var form = new PedsForm();
                    Application.Run(form);
                }
            }
            else if (path != null)
            {
                if (!NamedPipe.TrySendMessageToOtherProcess($"open-file {path}"))
                {
                    Form form = null;
                    try
                    {
                        form = OpenAnyFile.OpenFilePath(path);
                    }
                    catch (NotImplementedException ex)
                    {
                        MessageBox.Show("Dit type bestand is op het moment nog niet ondersteund!", ex.ToString());
                    }
                    if (form != null)
                    {
                        Application.Run(form);
                    }
                }
            }
            else
            {
                var form = new WorldForm();
                var namedPipe = new NamedPipe(form);
                namedPipe.StartServer();
                Application.Run(form);
            }
#if !DEBUG
        }
        catch (Exception ex)
        {
            MessageBox.Show("An unexpected error was encountered!\n" + ex.ToString());
            Console.WriteLine(ex);
            //this can happen if folder wasn't chosen, or in some other catastrophic error. meh.
        }
#endif
    }

    public static void LoadConfig()
    {
        using var _ = new DisposableTimer("LoadConfig");
        string configPath = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal).FilePath;
        if (!File.Exists(configPath))
        {
            Console.WriteLine("Upgrading config");
            //Existing user config does not exist, so load settings from previous assembly
            Settings.Default.Upgrade();
            Settings.Default.Reload();
            Settings.Default.Save();
        }
        //if (Settings.Default.UpgradeRequired)
        //{
        //Console.WriteLine($"Upgrading config {Settings.Default.UpgradeRequired}");
        //Settings.Default.SettingsSaving += (sender, args) =>
        //{
        //    Console.WriteLine($"Saving settings {Settings.Default.UpgradeRequired}");
        //};
        //Settings.Default.PropertyChanged += (sender, args) =>
        //{
        //    Console.WriteLine($"Property {args.PropertyName} Changed {Settings.Default[args.PropertyName]}");
        //};
        //Settings.Default.SettingChanging += (sender, args) =>
        //{
        //    Console.WriteLine($"Settings Changing {args.SettingName} from {Settings.Default[args.SettingName]} to {args.NewValue}");
        //};
        //    Settings.Default.Upgrade();
        //    Console.WriteLine(Settings.Default.UpgradeRequired);
        //    Settings.Default.UpgradeRequired = true;
        //    Settings.Default.UpgradeRequired = false;
        //    Settings.Default.Save();
        //}
    }

    static void EnsureJumpList()
    {
        if (Settings.Default.JumpListInitialised)
        {
            Console.WriteLine("Jump list already initialized");
            return;
        }

        try
        {
            Console.WriteLine("Initializing jump list");
            Application.OleRequired();
            var cwpath = Assembly.GetEntryAssembly().Location;
            var cwdir = Path.GetDirectoryName(cwpath);

            var jtWorld = new JumpTask();
            jtWorld.ApplicationPath = cwpath;
            jtWorld.IconResourcePath = cwpath;
            jtWorld.WorkingDirectory = cwdir;
            jtWorld.Arguments = "";
            jtWorld.Title = "World View";
            jtWorld.Description = "Display the GTAV World";
            jtWorld.CustomCategory = "Launch Options";

            var jtExplorer = new JumpTask();
            jtExplorer.ApplicationPath = cwpath;
            jtExplorer.IconResourcePath = Path.Combine(cwdir, "CodeWalker RPF Explorer.exe");
            jtExplorer.WorkingDirectory = cwdir;
            jtExplorer.Arguments = "explorer";
            jtExplorer.Title = "RPF Explorer";
            jtExplorer.Description = "Open RPF Explorer";
            jtExplorer.CustomCategory = "Launch Options";

            var jtVehicles = new JumpTask();
            jtVehicles.ApplicationPath = cwpath;
            jtVehicles.IconResourcePath = Path.Combine(cwdir, "CodeWalker Vehicle Viewer.exe");
            jtVehicles.WorkingDirectory = cwdir;
            jtVehicles.Arguments = "vehicles";
            jtVehicles.Title = "Vehicle Viewer";
            jtVehicles.Description = "Open Vehicle Viewer";
            jtVehicles.CustomCategory = "Launch Options";

            var jtPeds = new JumpTask();
            jtPeds.ApplicationPath = cwpath;
            jtPeds.IconResourcePath = Path.Combine(cwdir, "CodeWalker Ped Viewer.exe");
            jtPeds.WorkingDirectory = cwdir;
            jtPeds.Arguments = "peds";
            jtPeds.Title = "Ped Viewer";
            jtPeds.Description = "Open Ped Viewer";
            jtPeds.CustomCategory = "Launch Options";

            var jumpList = new JumpList();

            jumpList.JumpItems.Add(jtWorld);
            jumpList.JumpItems.Add(jtExplorer);
            jumpList.JumpItems.Add(jtVehicles);
            jumpList.JumpItems.Add(jtPeds);

            jumpList.Apply();

            Settings.Default.JumpListInitialised = true;
            Settings.Default.Save();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Initializing jump list failed {ex}");
        }
    }
}