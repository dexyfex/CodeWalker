using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CodeWalker.WinForms
{
    public partial class MenuStripFix : MenuStrip
    {
        public MenuStripFix()
        {
            InitializeComponent();
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            base.OnPaint(pe);
        }




        const uint WM_LBUTTONDOWN = 0x201;
        const uint WM_LBUTTONUP = 0x202;

        static private bool down = false;

        protected override void WndProc(ref Message m)
        {
            //fix to properly handle the mouse down event when the form isn't currently focused...
            if (m.Msg == WM_LBUTTONUP && !down)
            {
                m.Msg = (int)WM_LBUTTONDOWN;
                base.WndProc(ref m);
                m.Msg = (int)WM_LBUTTONUP;
            }

            if (m.Msg == WM_LBUTTONDOWN) down = true;
            if (m.Msg == WM_LBUTTONUP) down = false;

            base.WndProc(ref m);
        }
    }
}
