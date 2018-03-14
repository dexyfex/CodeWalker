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
    public partial class ToolStripSplitButtonFix : ToolStripSplitButton
    {
        //this allows the ToolStripSplitButton to have a "Checked" shaded appearance.
        private bool ischecked = false;

        /// <summary>
        /// Indictates the Checked state of the button.
        /// </summary>
        [Category("Behavior"),
        Description("Indicates whether or not the ToolStripSplitButtonFix is checked."),
        DefaultValue(false)]
        public bool Checked
        {
            get
            {
                return ischecked;
            }
            set
            {
                ischecked = value;
                this.Invalidate();
            }
        }

        public ToolStripSplitButtonFix()
        {
            InitializeComponent();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (ischecked)
            {
                ControlPaint.DrawBorder(e.Graphics,
                    e.ClipRectangle,            // To draw around the button + drop-down
                    //ButtonBounds,        // To draw only around the button 
                    SystemColors.MenuHighlight,
                    ButtonBorderStyle.Solid);
            }
        }
    }
}
