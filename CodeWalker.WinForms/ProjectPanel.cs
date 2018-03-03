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
            FormTheme.SetTheme(this, theme);
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
