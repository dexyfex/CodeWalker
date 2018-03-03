using CodeWalker.GameFiles;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CodeWalker.Project.Panels
{
    public partial class EditYndPanel : ProjectPanel
    {
        public ProjectForm ProjectForm;
        public YndFile Ynd { get; set; }

        private bool populatingui = false;
        private bool waschanged = false;

        public EditYndPanel(ProjectForm projectForm)
        {
            ProjectForm = projectForm;
            InitializeComponent();
        }

        public void SetYnd(YndFile ynd)
        {
            Ynd = ynd;
            Tag = ynd;
            UpdateFormTitle();
            UpdateYndUI();
            waschanged = ynd?.HasChanged ?? false;
        }

        public void UpdateFormTitleYndChanged()
        {
            bool changed = Ynd.HasChanged;
            if (!waschanged && changed)
            {
                UpdateFormTitle();
                waschanged = true;
            }
            else if (waschanged && !changed)
            {
                UpdateFormTitle();
                waschanged = false;
            }
        }
        private void UpdateFormTitle()
        {
            string fn = Ynd.RpfFileEntry?.Name ?? Ynd.Name;
            if (string.IsNullOrEmpty(fn)) fn = "untitled.ynd";
            Text = fn + (Ynd.HasChanged ? "*" : "");
        }


        public void UpdateYndUI()
        {
            if (Ynd == null)
            {
                //YndPanel.Enabled = false;
                YndRpfPathTextBox.Text = string.Empty;
                YndFilePathTextBox.Text = string.Empty;
                YndProjectPathTextBox.Text = string.Empty;
                YndAreaIDXUpDown.Value = 0;
                YndAreaIDYUpDown.Value = 0;
                YndAreaIDInfoLabel.Text = "ID: 0";
                YndTotalNodesLabel.Text = "Total Nodes: 0";
                YndVehicleNodesUpDown.Value = 0;
                YndVehicleNodesUpDown.Maximum = 0;
                YndPedNodesUpDown.Value = 0;
                YndPedNodesUpDown.Maximum = 0;
            }
            else
            {
                populatingui = true;
                var nd = Ynd.NodeDictionary;
                //YndPanel.Enabled = true;
                YndRpfPathTextBox.Text = Ynd.RpfFileEntry.Path;
                YndFilePathTextBox.Text = Ynd.FilePath;
                YndProjectPathTextBox.Text = (Ynd != null) ? ProjectForm.CurrentProjectFile.GetRelativePath(Ynd.FilePath) : Ynd.FilePath;
                YndAreaIDXUpDown.Value = Ynd.CellX;
                YndAreaIDYUpDown.Value = Ynd.CellY;
                YndAreaIDInfoLabel.Text = "ID: " + Ynd.AreaID.ToString();
                YndTotalNodesLabel.Text = "Total Nodes: " + (nd?.NodesCount.ToString() ?? "0");
                YndVehicleNodesUpDown.Maximum = nd?.NodesCount ?? 0;
                YndVehicleNodesUpDown.Value = Math.Min(nd?.NodesCountVehicle ?? 0, YndVehicleNodesUpDown.Maximum);
                YndPedNodesUpDown.Maximum = nd?.NodesCount ?? 0;
                YndPedNodesUpDown.Value = Math.Min(nd?.NodesCountPed ?? 0, YndPedNodesUpDown.Maximum);
                populatingui = false;
            }
        }

        private void YndAreaIDUpDownChange()
        {
            if (populatingui) return;
            if (Ynd == null) return;
            int x = (int)YndAreaIDXUpDown.Value;
            int y = (int)YndAreaIDYUpDown.Value;
            lock (ProjectForm.ProjectSyncRoot)
            {
                var areaid = y * 32 + x;
                if (Ynd.AreaID != areaid)
                {
                    Ynd.AreaID = areaid;
                    Ynd.Name = "nodes" + areaid.ToString() + ".ynd";
                    YndAreaIDInfoLabel.Text = "ID: " + areaid.ToString();
                    ProjectForm.SetYndHasChanged(true);
                }
            }
        }

        private void YndAreaIDXUpDown_ValueChanged(object sender, EventArgs e)
        {
            YndAreaIDUpDownChange();
        }

        private void YndAreaIDYUpDown_ValueChanged(object sender, EventArgs e)
        {
            YndAreaIDUpDownChange();
        }

        private void YndVehicleNodesUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (Ynd == null) return;
            if (Ynd.NodeDictionary == null) return;
            lock (ProjectForm.ProjectSyncRoot)
            {
                var vehnodes = (int)YndVehicleNodesUpDown.Value;
                if (Ynd.NodeDictionary.NodesCountVehicle != vehnodes)
                {
                    Ynd.NodeDictionary.NodesCountVehicle = (uint)vehnodes;
                    ProjectForm.SetYndHasChanged(true);
                }
            }
        }

        private void YndPedNodesUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (Ynd == null) return;
            if (Ynd.NodeDictionary == null) return;
            lock (ProjectForm.ProjectSyncRoot)
            {
                var pednodes = (int)YndPedNodesUpDown.Value;
                if (Ynd.NodeDictionary.NodesCountPed != pednodes)
                {
                    Ynd.NodeDictionary.NodesCountPed = (uint)pednodes;
                    ProjectForm.SetYndHasChanged(true);
                }
            }
        }
    }
}
