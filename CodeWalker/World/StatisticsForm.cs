using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CodeWalker.World
{
    public partial class StatisticsForm : Form
    {
        private WorldForm worldForm;

        public StatisticsForm(WorldForm wf)
        {
            worldForm = wf;
            InitializeComponent();
        }

        private void DoneButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void MainTimer_Tick(object sender, EventArgs e)
        {
            if (worldForm == null) return;

            var gfc = worldForm.GameFileCache;
            var rc = worldForm.Renderer.RenderableCache;

            GFCQueueLengthLabel.Text = gfc.QueueLength.ToString();
            GFCItemCountLabel.Text = gfc.ItemCount.ToString();
            GFCMemoryUsageLabel.Text = TextUtil.GetBytesReadable(gfc.MemoryUsage);

            RCQueueLengthLabel.Text = rc.TotalQueueLength.ToString();
            RCItemCountLabel.Text = rc.TotalItemCount.ToString();
            RCVramUsageLabel.Text = TextUtil.GetBytesReadable(rc.TotalGraphicsMemoryUse);

        }
    }
}
