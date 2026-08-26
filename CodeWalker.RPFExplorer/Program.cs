using CodeWalker.GameFiles;
using CodeWalker.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CodeWalker.RPFExplorer
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            EmbeddedAssemblies.Install();
            UserConfig.Repair();
            Run(args);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        static void Run(string[] args)
        {
            //Process.Start("CodeWalker.exe", "explorer");

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            DarkTheme.Install();

            var form = new ExploreForm();
            if (args != null)
            {
                foreach (var arg in args)
                {
                    if (string.IsNullOrEmpty(arg)) continue;
                    form.StartupPaths.Add(arg);
                }
            }

            Application.Run(form);

            GTAFolder.UpdateSettings();
        }
    }
}
