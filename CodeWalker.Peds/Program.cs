using CodeWalker.GameFiles;
using CodeWalker.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CodeWalker.Peds
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            EmbeddedAssemblies.Install();
            UserConfig.Repair();
            Run();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        static void Run()
        {
            //Process.Start("CodeWalker.exe", "peds");

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            DarkTheme.Install();
            Application.Run(new PedsForm());

            GTAFolder.UpdateSettings();
        }
    }
}
