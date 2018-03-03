using CodeWalker.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CodeWalker
{
    public partial class MenuForm : Form
    {
        private volatile bool worldFormOpen = false;
        private WorldForm worldForm = null;

        public MenuForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            Settings.Default.Save();
        }

        private void RPFExplorerButton_Click(object sender, EventArgs e)
        {
            ExploreForm f = new ExploreForm();
            f.Show(this);
        }

        private void RPFBrowserButton_Click(object sender, EventArgs e)
        {
            BrowseForm f = new BrowseForm();
            f.Show(this);
        }

        private void ExtractScriptsButton_Click(object sender, EventArgs e)
        {
            ExtractScriptsForm f = new ExtractScriptsForm();
            f.Show(this);
        }

        private void ExtractTexturesButton_Click(object sender, EventArgs e)
        {
            ExtractTexForm f = new ExtractTexForm();
            f.Show(this);
        }

        private void ExtractRawFilesButton_Click(object sender, EventArgs e)
        {
            ExtractRawForm f = new ExtractRawForm();
            f.Show(this);
        }

        private void ExtractShadersButton_Click(object sender, EventArgs e)
        {
            ExtractShadersForm f = new ExtractShadersForm();
            f.Show(this);
        }

        private void BinarySearchButton_Click(object sender, EventArgs e)
        {
            BinarySearchForm f = new BinarySearchForm();
            f.Show(this);
        }

        private void WorldButton_Click(object sender, EventArgs e)
        {
            if (worldFormOpen)
            {
                //MessageBox.Show("Can only open one world view at a time.");
                if (worldForm != null)
                {
                    worldForm.Invoke(new Action(() => { worldForm.Focus(); }));
                }
                return;
            }

            Thread thread = new Thread(new ThreadStart(() => {
                try
                {
                    worldFormOpen = true;
                    using (WorldForm f = new WorldForm())
                    {
                        worldForm = f;
                        f.ShowDialog();
                        worldForm = null;
                    }
                    worldFormOpen = false;
                }
                catch
                {
                    worldFormOpen = false;
                }
            }));
            thread.Start();
        }

        private void GCCollectButton_Click(object sender, EventArgs e)
        {
            GC.Collect();
        }

        private void AboutButton_Click(object sender, EventArgs e)
        {
            AboutForm f = new AboutForm();
            f.Show(this);
        }

        private void JenkGenButton_Click(object sender, EventArgs e)
        {
            JenkGenForm f = new JenkGenForm();
            f.Show(this);
        }

        private void JenkIndButton_Click(object sender, EventArgs e)
        {
            JenkIndForm f = new JenkIndForm();
            f.Show(this);
        }

        private void ExtractKeysButton_Click(object sender, EventArgs e)
        {
            ExtractKeysForm f = new ExtractKeysForm();
            f.Show(this);
        }

        private void ProjectButton_Click(object sender, EventArgs e)
        {
            Project.ProjectForm f = new Project.ProjectForm(null);
            f.Show(this);
        }
    }
}
