using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace CodeWalker.Project
{
    public class ProjectPanel : DockContent
    {


        public virtual void SetTheme(ThemeBase theme)
        {
            BackColor = SystemColors.Control;
            ForeColor = SystemColors.ControlText;
            var txtback = SystemColors.Window;
            var disback = SystemColors.Control;
            var disfore = ForeColor;
            var btnback = Color.Transparent;

            if (theme is VS2015DarkTheme)
            {
                BackColor = theme.ColorPalette.MainWindowActive.Background;
                ForeColor = Color.White;
                txtback = BackColor;
                disback = BackColor;// Color.FromArgb(32,32,32);
                disfore = Color.DarkGray;
                btnback = BackColor;
            }

            var allcontrols = new List<Control>();
            RecurseControls(this, allcontrols);

            foreach (var c in allcontrols)
            {
                if ((c is TabPage) || (c is CheckedListBox) || (c is ListBox))
                {
                    c.ForeColor = ForeColor;
                    c.BackColor = txtback;
                }
                else if ((c is TextBox))
                {
                    var txtbox = c as TextBox;
                    c.ForeColor = txtbox.ReadOnly ? disfore : ForeColor;
                    c.BackColor = txtbox.ReadOnly ? disback : txtback;
                }
                else if ((c is Button) || (c is GroupBox))
                {
                    c.ForeColor = ForeColor;
                    c.BackColor = btnback;
                }
            }

        }

        private void RecurseControls(Control c, List<Control> l)
        {
            foreach (Control cc in c.Controls)
            {
                l.Add(cc);
                RecurseControls(cc, l);
            }
        }

    }


    public class ProjectFloatWindow : FloatWindow
    {
        public ProjectFloatWindow(DockPanel dockPanel, DockPane pane)
            : base(dockPanel, pane)
        {
            Init(dockPanel, pane);
        }

        public ProjectFloatWindow(DockPanel dockPanel, DockPane pane, Rectangle bounds)
            : base(dockPanel, pane, bounds)
        {
            Init(dockPanel, pane);
        }

        private void Init(DockPanel dockPanel, DockPane pane)
        {
            FormBorderStyle = FormBorderStyle.Sizable;

            //trying to set float window tab strip location to the top... for some reason it defaults to bottom!
            //var t = this;
            //if (pane != null)
            //{
            //    pane.DockState = DockState.Document;
            //}
            //if (dockPanel != null)
            //{
            //    dockPanel.DocumentTabStripLocation = DocumentTabStripLocation.Top;
            //}
            //if (DockPanel != null)
            //{
            //    DockPanel.DocumentTabStripLocation = DocumentTabStripLocation.Top;
            //}
        }
    }
    public class ProjectFloatWindowFactory : DockPanelExtender.IFloatWindowFactory
    {
        public FloatWindow CreateFloatWindow(DockPanel dockPanel, DockPane pane, Rectangle bounds)
        {
            return new ProjectFloatWindow(dockPanel, pane, bounds);
        }

        public FloatWindow CreateFloatWindow(DockPanel dockPanel, DockPane pane)
        {
            return new ProjectFloatWindow(dockPanel, pane);
        }
    }
}
