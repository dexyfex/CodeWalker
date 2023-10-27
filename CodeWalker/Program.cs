﻿using CodeWalker.Forms;
using CodeWalker.GameFiles;
using CodeWalker.Properties;
using CodeWalker.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Shell;

namespace CodeWalker
{
    static class Program
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool AllocConsole();
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            ConsoleWindow.Hide();
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
                    Application.Run(new ExploreForm());
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
                    Application.Run(new PedsForm());
                }
                else if (path != null)
                {
                    var modelForm = new ModelForm();
                    modelForm.Load += new EventHandler(async (sender, eventArgs) => {
                        modelForm.ViewModel(path);
                    });
                    Application.Run(modelForm);
                }
                else
                {
                    Application.Run(new WorldForm());
                }
#if !DEBUG
            }
            catch (Exception ex)
            {
                MessageBox.Show("An unexpected error was encountered!\n" + ex.ToString());
                //this can happen if folder wasn't chosen, or in some other catastrophic error. meh.
            }
#endif
        }


        static void EnsureJumpList()
        {
            if (Settings.Default.JumpListInitialised) return;

            try
            {
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
            catch
            { }
        }
    }
}
