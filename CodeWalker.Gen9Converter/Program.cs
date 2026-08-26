using CodeWalker.Tools;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CodeWalker.Gen9Converter
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
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            DarkTheme.Install();
            Application.Run(new ConvertAssetsForm());
        }
    }
}