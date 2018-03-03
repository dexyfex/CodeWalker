using CodeWalker.GameFiles;
using CodeWalker.World;
using SharpDX;
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
    public partial class EditTrainNodePanel : ProjectPanel
    {
        public ProjectForm ProjectForm;
        public TrainTrackNode TrainNode { get; set; }

        private bool populatingui = false;

        public EditTrainNodePanel(ProjectForm projectForm)
        {
            ProjectForm = projectForm;
            InitializeComponent();
        }

        public void SetTrainNode(TrainTrackNode node)
        {
            TrainNode = node;
            Tag = node;
            UpdateFormTitle();
            UpdateTrainTrackNodeUI();
        }

        private void UpdateFormTitle()
        {
            Text = "Train Node " + TrainNode.Index.ToString();
        }

        public void UpdateTrainTrackNodeUI()
        {
            if (TrainNode == null)
            {
                //TrainNodePanel.Enabled = false;
                TrainNodeDeleteButton.Enabled = false;
                TrainNodeAddToProjectButton.Enabled = false;
                TrainNodePositionTextBox.Text = string.Empty;
                TrainNodeTypeComboBox.SelectedIndex = -1;
            }
            else
            {
                populatingui = true;
                //TrainNodePanel.Enabled = true;
                TrainNodeDeleteButton.Enabled = ProjectForm.TrainTrackExistsInProject(TrainNode.Track);
                TrainNodeAddToProjectButton.Enabled = !TrainNodeDeleteButton.Enabled;
                TrainNodePositionTextBox.Text = FloatUtil.GetVector3String(TrainNode.Position);
                TrainNodeTypeComboBox.SelectedIndex = TrainNode.NodeType;
                populatingui = false;

                if (ProjectForm.WorldForm != null)
                {
                    ProjectForm.WorldForm.SelectTrainTrackNode(TrainNode);
                }
            }
        }

        private void TrainNodePositionTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (TrainNode == null) return;
            Vector3 v = FloatUtil.ParseVector3String(TrainNodePositionTextBox.Text);
            bool change = false;
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (TrainNode.Position != v)
                {
                    TrainNode.SetPosition(v);
                    ProjectForm.SetTrainTrackHasChanged(true);
                    change = true;
                }
            }
            if (change)
            {
                if (ProjectForm.WorldForm != null)
                {
                    ProjectForm.WorldForm.SetWidgetPosition(TrainNode.Position);
                    ProjectForm.WorldForm.UpdateTrainTrackNodeGraphics(TrainNode, false);
                }
                //TrainNodePositionTextBox.Text = FloatUtil.GetVector3String(CurrentTrainNode.Position);
            }
        }

        private void TrainNodeGoToButton_Click(object sender, EventArgs e)
        {
            if (TrainNode == null) return;
            if (ProjectForm.WorldForm == null) return;
            ProjectForm.WorldForm.GoToPosition(TrainNode.Position);
        }

        private void TrainNodeTypeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (TrainNode == null) return;
            int type = TrainNodeTypeComboBox.SelectedIndex;
            bool change = false;
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (TrainNode.NodeType != type)
                {
                    TrainNode.NodeType = type;
                    ProjectForm.SetTrainTrackHasChanged(true);
                    change = true;
                }
            }
            if (change)
            {
                if (ProjectForm.WorldForm != null)
                {
                    ProjectForm.WorldForm.UpdateTrainTrackNodeGraphics(TrainNode, false); //change the colour...
                }
            }
            ProjectForm.ProjectExplorer?.UpdateTrainNodeTreeNode(TrainNode);
        }

        private void TrainNodeAddToProjectButton_Click(object sender, EventArgs e)
        {
            ProjectForm.SetProjectItem(TrainNode);
            ProjectForm.AddTrainTrackToProject(TrainNode.Track);
        }

        private void TrainNodeDeleteButton_Click(object sender, EventArgs e)
        {
            ProjectForm.SetProjectItem(TrainNode);
            ProjectForm.DeleteTrainNode();
        }
    }
}
