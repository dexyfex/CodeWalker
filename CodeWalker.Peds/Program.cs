using CodeWalker.Core.Utils;
using CodeWalker.GameFiles;
using CodeWalker.Properties;
using CodeWalker.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
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
            //Process.Start("CodeWalker.exe", "peds");

            if (!NamedPipe.TrySendMessageToOtherProcess("peds-mode"))
            {
                ConsoleWindow.Hide();
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new PedsForm());

                GTAFolder.UpdateSettings();
            }
        }
    }
}
