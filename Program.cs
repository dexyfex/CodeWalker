using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CodeWalker
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {

            bool menumode = false;
            bool explorermode = false;
            if ((args != null) && (args.Length > 0))
            {
                foreach (string arg in args)
                {
                    if (arg.ToLower() == "menu")
                    {
                        menumode = true;
                    }
                    if (arg.ToLower() == "explorer")
                    {
                        explorermode = true;
                    }
                }
            }
            
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

#if !DEBUG
            try
            {
#endif
                if (menumode)
                {
                    Application.Run(new MainForm());
                }
                else if (explorermode)
                {
                    Application.Run(new ExploreForm());
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
    }
}
