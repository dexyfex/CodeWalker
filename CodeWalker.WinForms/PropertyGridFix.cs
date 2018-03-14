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
    public partial class PropertyGridFix : PropertyGrid
    {
        public PropertyGridFix()
        {
            InitializeComponent();
            LineColor = SystemColors.InactiveBorder; //makes it consistent across windows 10 versions........
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            base.OnPaint(pe);
        }
    }
}
