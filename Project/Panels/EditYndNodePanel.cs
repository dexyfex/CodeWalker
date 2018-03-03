using CodeWalker.GameFiles;
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
    public partial class EditYndNodePanel : ProjectPanel
    {
        public ProjectForm ProjectForm;
        public YndNode CurrentPathNode { get; set; }
        public YndLink CurrentPathLink { get; set; }
        public YndFile CurrentYndFile { get; set; }

        private bool populatingui = false;

        public EditYndNodePanel(ProjectForm projectForm)
        {
            ProjectForm = projectForm;
            InitializeComponent();
        }

        public void SetPathNode(YndNode node)
        {
            CurrentPathNode = node;
            CurrentPathLink = null;
            CurrentYndFile = node?.Ynd;
            Tag = node;
            UpdateFormTitle();
            UpdateYndNodeUI();
        }

        private void UpdateFormTitle()
        {
            var sn = CurrentPathNode.StreetName.Hash == 0 ? "Path node" : CurrentPathNode.StreetName.ToString();
            Text = sn + " " + CurrentPathNode.NodeID.ToString();
        }

        public void UpdateYndNodeUI()
        {
            LoadPathNodeTabPage();

            LoadPathNodeJunctionPage();

            LoadPathNodeLinkPage();
        }


        private void LoadPathNodeTabPage()
        {

            CurrentPathLink = null;

            if (CurrentPathNode == null)
            {
                //YndNodePanel.Enabled = false;
                PathNodeDeleteButton.Enabled = false;
                PathNodeAddToProjectButton.Enabled = false;
                PathNodeAreaIDUpDown.Value = 0;
                PathNodeNodeIDUpDown.Value = 0;
                PathNodePositionTextBox.Text = string.Empty;
                PathNodeStreetHashTextBox.Text = string.Empty;
                PathNodeStreetNameLabel.Text = "Name: [None]";

                UpdatePathNodeFlagsUI(true, true);

                PathNodeLinkCountLabel.Text = "Link Count: 0";
                PathNodeLinksListBox.Items.Clear();

            }
            else
            {
                populatingui = true;
                var n = CurrentPathNode.RawData;
                //YndNodePanel.Enabled = true;
                PathNodeDeleteButton.Enabled = ProjectForm.YndExistsInProject(CurrentYndFile);
                PathNodeAddToProjectButton.Enabled = !PathNodeDeleteButton.Enabled;
                var streetname = GlobalText.TryGetString(n.StreetName.Hash);
                PathNodeAreaIDUpDown.Value = n.AreaID;
                PathNodeNodeIDUpDown.Value = n.NodeID;
                PathNodePositionTextBox.Text = FloatUtil.GetVector3String(CurrentPathNode.Position);
                PathNodeStreetHashTextBox.Text = n.StreetName.Hash.ToString();
                PathNodeStreetNameLabel.Text = "Name: " + ((n.StreetName.Hash == 0) ? "[None]" : (string.IsNullOrEmpty(streetname) ? "[Not found]" : streetname));

                UpdatePathNodeFlagsUI(true, true);

                PathNodeLinkCountLabel.Text = "Link Count: " + CurrentPathNode.LinkCount.ToString();
                PathNodeLinksListBox.Items.Clear();
                if (CurrentPathNode.Links != null)
                {
                    foreach (var link in CurrentPathNode.Links)
                    {
                        PathNodeLinksListBox.Items.Add(link);
                    }
                }
                populatingui = false;


                if (ProjectForm.WorldForm != null)
                {
                    ProjectForm.WorldForm.SelectPathNode(CurrentPathNode);
                }

            }
        }

        private void LoadPathNodeLinkPage()
        {
            if (CurrentPathLink == null)
            {
                PathNodeLinkPanel.Enabled = false;
                PathNodeLinkAreaIDUpDown.Value = 0;
                PathNodeLinkNodeIDUpDown.Value = 0;

                UpdatePathNodeLinkFlagsUI(true, true);

                PathNodeLinkLengthUpDown.Value = 0;
                PathNodeLinkageStatusLabel.Text = "";
            }
            else
            {
                populatingui = true;
                PathNodeLinkPanel.Enabled = true;
                PathNodeLinkAreaIDUpDown.Value = CurrentPathLink._RawData.AreaID;
                PathNodeLinkNodeIDUpDown.Value = CurrentPathLink._RawData.NodeID;

                UpdatePathNodeLinkFlagsUI(true, true);

                PathNodeLinkLengthUpDown.Value = CurrentPathLink.LinkLength.Value;
                PathNodeLinkageStatusLabel.Text = "";
                populatingui = false;

                if (ProjectForm.WorldForm != null)
                {
                    ProjectForm.WorldForm.SelectPathLink(CurrentPathLink);
                }
            }

        }

        private void LoadPathNodeJunctionPage()
        {

            var junc = CurrentPathNode?.Junction;
            if (junc == null)
            {
                PathNodeJunctionEnableCheckBox.Checked = false;
                PathNodeJunctionPanel.Enabled = false;
                PathNodeJunctionMaxZUpDown.Value = 0;
                PathNodeJunctionMinZUpDown.Value = 0;
                PathNodeJunctionPosXUpDown.Value = 0;
                PathNodeJunctionPosYUpDown.Value = 0;
                PathNodeJunctionHeightmapDimXUpDown.Value = 1;
                PathNodeJunctionHeightmapDimYUpDown.Value = 1;
                PathNodeJunctionHeightmapBytesTextBox.Text = string.Empty;
            }
            else
            {
                populatingui = true;
                PathNodeJunctionEnableCheckBox.Checked = CurrentPathNode.HasJunction;
                PathNodeJunctionPanel.Enabled = PathNodeJunctionEnableCheckBox.Checked;
                PathNodeJunctionMaxZUpDown.Value = junc.MaxZ;
                PathNodeJunctionMinZUpDown.Value = junc.MinZ;
                PathNodeJunctionPosXUpDown.Value = junc.PositionX;
                PathNodeJunctionPosYUpDown.Value = junc.PositionY;
                PathNodeJunctionHeightmapDimXUpDown.Value = junc.Heightmap.CountX;
                PathNodeJunctionHeightmapDimYUpDown.Value = junc.Heightmap.CountY;
                PathNodeJunctionHeightmapBytesTextBox.Text = junc.Heightmap?.GetDataString() ?? "";
                populatingui = false;
            }


        }

        private void UpdatePathNodeFlagsUI(bool updateCheckboxes, bool updateUpDowns)
        {

            var flags0 = CurrentPathNode?.Flags0.Value ?? 0;
            var flags1 = CurrentPathNode?.Flags1.Value ?? 0;
            var flags2 = CurrentPathNode?.Flags2.Value ?? 0;
            var flags3 = CurrentPathNode?.Flags3.Value ?? 0;
            var flags4 = CurrentPathNode?.Flags4.Value ?? 0;
            var flags5 = (uint)(CurrentPathNode?.LinkCountUnk ?? 0);


            if (updateCheckboxes)
            {
                PathNodeFlags01CheckBox.Checked = BitUtil.IsBitSet(flags0, 0);
                PathNodeFlags02CheckBox.Checked = BitUtil.IsBitSet(flags0, 1);
                PathNodeFlags03CheckBox.Checked = BitUtil.IsBitSet(flags0, 2);
                PathNodeFlags04CheckBox.Checked = BitUtil.IsBitSet(flags0, 3);
                PathNodeFlags05CheckBox.Checked = BitUtil.IsBitSet(flags0, 4);
                PathNodeFlags06CheckBox.Checked = BitUtil.IsBitSet(flags0, 5);
                PathNodeFlags07CheckBox.Checked = BitUtil.IsBitSet(flags0, 6);
                PathNodeFlags08CheckBox.Checked = BitUtil.IsBitSet(flags0, 7);

                PathNodeFlags11CheckBox.Checked = BitUtil.IsBitSet(flags1, 0);
                PathNodeFlags12CheckBox.Checked = BitUtil.IsBitSet(flags1, 1);
                PathNodeFlags13CheckBox.Checked = BitUtil.IsBitSet(flags1, 2);
                PathNodeFlags14CheckBox.Checked = BitUtil.IsBitSet(flags1, 3);
                PathNodeFlags15CheckBox.Checked = BitUtil.IsBitSet(flags1, 4);
                PathNodeFlags16CheckBox.Checked = BitUtil.IsBitSet(flags1, 5);
                PathNodeFlags17CheckBox.Checked = BitUtil.IsBitSet(flags1, 6);
                PathNodeFlags18CheckBox.Checked = BitUtil.IsBitSet(flags1, 7);

                PathNodeFlags21CheckBox.Checked = BitUtil.IsBitSet(flags2, 0);
                PathNodeFlags22CheckBox.Checked = BitUtil.IsBitSet(flags2, 1);
                PathNodeFlags23CheckBox.Checked = BitUtil.IsBitSet(flags2, 2);
                PathNodeFlags24CheckBox.Checked = BitUtil.IsBitSet(flags2, 3);
                PathNodeFlags25CheckBox.Checked = BitUtil.IsBitSet(flags2, 4);
                PathNodeFlags26CheckBox.Checked = BitUtil.IsBitSet(flags2, 5);
                PathNodeFlags27CheckBox.Checked = BitUtil.IsBitSet(flags2, 6);
                PathNodeFlags28CheckBox.Checked = BitUtil.IsBitSet(flags2, 7);

                PathNodeFlags31CheckBox.Checked = BitUtil.IsBitSet(flags3, 0);
                PathNodeFlags32UpDown.Value = (flags3 >> 1) & 127;

                PathNodeFlags41CheckBox.Checked = BitUtil.IsBitSet(flags4, 0);
                PathNodeFlags42UpDown.Value = (flags4 >> 1) & 7;
                PathNodeFlags45CheckBox.Checked = BitUtil.IsBitSet(flags4, 4);
                PathNodeFlags46CheckBox.Checked = BitUtil.IsBitSet(flags4, 5);
                PathNodeFlags47CheckBox.Checked = BitUtil.IsBitSet(flags4, 6);
                PathNodeFlags48CheckBox.Checked = BitUtil.IsBitSet(flags4, 7);

                PathNodeFlags51CheckBox.Checked = BitUtil.IsBitSet(flags5, 0);
                PathNodeFlags52CheckBox.Checked = BitUtil.IsBitSet(flags5, 1);
                PathNodeFlags53CheckBox.Checked = BitUtil.IsBitSet(flags5, 2);
            }
            if (updateUpDowns)
            {
                PathNodeFlags0UpDown.Value = flags0;
                PathNodeFlags1UpDown.Value = flags1;
                PathNodeFlags2UpDown.Value = flags2;
                PathNodeFlags3UpDown.Value = flags3;
                PathNodeFlags4UpDown.Value = flags4;
                PathNodeFlags5UpDown.Value = flags5;
            }

            var n = CurrentPathNode;
            if (n != null)
            {
                PathNodeFlags0Label.Text = n.Flags0.ToHexString();
                PathNodeFlags1Label.Text = n.Flags1.ToHexString();
                PathNodeFlags2Label.Text = n.Flags2.ToHexString();
                PathNodeFlags3Label.Text = n.Flags3.ToHexString();
                PathNodeFlags4Label.Text = n.Flags4.ToHexString();
            }
            else
            {
                PathNodeFlags0Label.Text = "0x00";
                PathNodeFlags1Label.Text = "0x00";
                PathNodeFlags2Label.Text = "0x00";
                PathNodeFlags3Label.Text = "0x00";
                PathNodeFlags4Label.Text = "0x00";
            }
        }

        private void UpdatePathNodeLinkFlagsUI(bool updateCheckboxes, bool updateUpDowns)
        {
            var flags0 = CurrentPathLink?.Flags0.Value ?? 0;
            var flags1 = CurrentPathLink?.Flags1.Value ?? 0;
            var flags2 = CurrentPathLink?.Flags2.Value ?? 0;


            if (updateCheckboxes)
            {
                PathNodeLinkFlags01CheckBox.Checked = BitUtil.IsBitSet(flags0, 0);
                PathNodeLinkFlags02CheckBox.Checked = BitUtil.IsBitSet(flags0, 1);
                PathNodeLinkFlags03UpDown.Value = (flags0 >> 2) & 7;
                PathNodeLinkFlags04UpDown.Value = (flags0 >> 5) & 7;

                PathNodeLinkFlags11CheckBox.Checked = BitUtil.IsBitSet(flags1, 0);
                PathNodeLinkFlags12CheckBox.Checked = BitUtil.IsBitSet(flags1, 1);
                PathNodeLinkFlags13CheckBox.Checked = BitUtil.IsBitSet(flags1, 2);
                PathNodeLinkFlags14CheckBox.Checked = BitUtil.IsBitSet(flags1, 3);
                PathNodeLinkOffsetSizeUpDown.Value = (flags1 >> 4) & 7;
                PathNodeLinkFlags18CheckBox.Checked = BitUtil.IsBitSet(flags1, 7);

                PathNodeLinkFlags21CheckBox.Checked = BitUtil.IsBitSet(flags2, 0);
                PathNodeLinkFlags22CheckBox.Checked = BitUtil.IsBitSet(flags2, 1);
                PathNodeLinkBackLanesUpDown.Value = (flags2 >> 2) & 7;
                PathNodeLinkFwdLanesUpDown.Value = (flags2 >> 5) & 7;
            }
            if (updateUpDowns)
            {
                PathNodeLinkFlags0UpDown.Value = flags0;
                PathNodeLinkFlags1UpDown.Value = flags1;
                PathNodeLinkFlags2UpDown.Value = flags2;
            }

            var l = CurrentPathLink;
            if (l != null)
            {
                PathNodeLinkFlags0Label.Text = l.Flags0.ToHexString();
                PathNodeLinkFlags1Label.Text = l.Flags1.ToHexString();
                PathNodeLinkFlags2Label.Text = l.Flags2.ToHexString();
            }
            else
            {
                PathNodeLinkFlags0Label.Text = "0x00";
                PathNodeLinkFlags1Label.Text = "0x00";
                PathNodeLinkFlags2Label.Text = "0x00";
            }
        }

        private void SetPathNodeFlagsFromCheckBoxes()
        {
            if (populatingui) return;
            if (CurrentPathNode == null) return;

            uint flags0 = 0;
            uint flags1 = 0;
            uint flags2 = 0;
            uint flags3 = 0;
            uint flags4 = 0;
            uint flags5 = 0;
            flags0 = BitUtil.UpdateBit(flags0, 0, PathNodeFlags01CheckBox.Checked);
            flags0 = BitUtil.UpdateBit(flags0, 1, PathNodeFlags02CheckBox.Checked);
            flags0 = BitUtil.UpdateBit(flags0, 2, PathNodeFlags03CheckBox.Checked);
            flags0 = BitUtil.UpdateBit(flags0, 3, PathNodeFlags04CheckBox.Checked);
            flags0 = BitUtil.UpdateBit(flags0, 4, PathNodeFlags05CheckBox.Checked);
            flags0 = BitUtil.UpdateBit(flags0, 5, PathNodeFlags06CheckBox.Checked);
            flags0 = BitUtil.UpdateBit(flags0, 6, PathNodeFlags07CheckBox.Checked);
            flags0 = BitUtil.UpdateBit(flags0, 7, PathNodeFlags08CheckBox.Checked);

            flags1 = BitUtil.UpdateBit(flags1, 0, PathNodeFlags11CheckBox.Checked);
            flags1 = BitUtil.UpdateBit(flags1, 1, PathNodeFlags12CheckBox.Checked);
            flags1 = BitUtil.UpdateBit(flags1, 2, PathNodeFlags13CheckBox.Checked);
            flags1 = BitUtil.UpdateBit(flags1, 3, PathNodeFlags14CheckBox.Checked);
            flags1 = BitUtil.UpdateBit(flags1, 4, PathNodeFlags15CheckBox.Checked);
            flags1 = BitUtil.UpdateBit(flags1, 5, PathNodeFlags16CheckBox.Checked);
            flags1 = BitUtil.UpdateBit(flags1, 6, PathNodeFlags17CheckBox.Checked);
            flags1 = BitUtil.UpdateBit(flags1, 7, PathNodeFlags18CheckBox.Checked);

            flags2 = BitUtil.UpdateBit(flags2, 0, PathNodeFlags21CheckBox.Checked);
            flags2 = BitUtil.UpdateBit(flags2, 1, PathNodeFlags22CheckBox.Checked);
            flags2 = BitUtil.UpdateBit(flags2, 2, PathNodeFlags23CheckBox.Checked);
            flags2 = BitUtil.UpdateBit(flags2, 3, PathNodeFlags24CheckBox.Checked);
            flags2 = BitUtil.UpdateBit(flags2, 4, PathNodeFlags25CheckBox.Checked);
            flags2 = BitUtil.UpdateBit(flags2, 5, PathNodeFlags26CheckBox.Checked);
            flags2 = BitUtil.UpdateBit(flags2, 6, PathNodeFlags27CheckBox.Checked);
            flags2 = BitUtil.UpdateBit(flags2, 7, PathNodeFlags28CheckBox.Checked);

            flags3 = BitUtil.UpdateBit(flags3, 0, PathNodeFlags31CheckBox.Checked);
            flags3 += (((uint)PathNodeFlags32UpDown.Value & 127u) << 1);

            flags4 = BitUtil.UpdateBit(flags4, 0, PathNodeFlags41CheckBox.Checked);
            flags4 += (((uint)PathNodeFlags42UpDown.Value & 7u) << 1);
            flags4 = BitUtil.UpdateBit(flags4, 4, PathNodeFlags45CheckBox.Checked);
            flags4 = BitUtil.UpdateBit(flags4, 5, PathNodeFlags46CheckBox.Checked);
            flags4 = BitUtil.UpdateBit(flags4, 6, PathNodeFlags47CheckBox.Checked);
            flags4 = BitUtil.UpdateBit(flags4, 7, PathNodeFlags48CheckBox.Checked);

            flags5 = BitUtil.UpdateBit(flags5, 0, PathNodeFlags51CheckBox.Checked);
            flags5 = BitUtil.UpdateBit(flags5, 1, PathNodeFlags52CheckBox.Checked);
            flags5 = BitUtil.UpdateBit(flags5, 2, PathNodeFlags53CheckBox.Checked);


            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CurrentPathNode.Flags0.Value != flags0)
                {
                    CurrentPathNode.Flags0 = (byte)flags0;
                    ProjectForm.SetYndHasChanged(true);
                }
                if (CurrentPathNode.Flags1.Value != flags1)
                {
                    CurrentPathNode.Flags1 = (byte)flags1;
                    ProjectForm.SetYndHasChanged(true);
                }
                if (CurrentPathNode.Flags2.Value != flags2)
                {
                    CurrentPathNode.Flags2 = (byte)flags2;
                    ProjectForm.SetYndHasChanged(true);
                }
                if (CurrentPathNode.Flags3.Value != flags3)
                {
                    CurrentPathNode.Flags3 = (byte)flags3;
                    ProjectForm.SetYndHasChanged(true);
                }
                if (CurrentPathNode.Flags4.Value != flags4)
                {
                    CurrentPathNode.Flags4 = (byte)flags4;
                    ProjectForm.SetYndHasChanged(true);
                }
                if (CurrentPathNode.LinkCountUnk != flags5)
                {
                    CurrentPathNode.LinkCountUnk = (byte)flags5;
                    ProjectForm.SetYndHasChanged(true);
                }
            }

            populatingui = true;
            UpdatePathNodeFlagsUI(false, true); //update updowns
            populatingui = false;
        }

        private void SetPathNodeFlagsFromUpDowns()
        {
            if (populatingui) return;
            if (CurrentPathNode == null) return;

            uint flags0 = (uint)PathNodeFlags0UpDown.Value;
            uint flags1 = (uint)PathNodeFlags1UpDown.Value;
            uint flags2 = (uint)PathNodeFlags2UpDown.Value;
            uint flags3 = (uint)PathNodeFlags3UpDown.Value;
            uint flags4 = (uint)PathNodeFlags4UpDown.Value;
            uint flags5 = (uint)PathNodeFlags5UpDown.Value;

            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CurrentPathNode.Flags0.Value != flags0)
                {
                    CurrentPathNode.Flags0 = (byte)flags0;
                    ProjectForm.SetYndHasChanged(true);
                }
                if (CurrentPathNode.Flags1.Value != flags1)
                {
                    CurrentPathNode.Flags1 = (byte)flags1;
                    ProjectForm.SetYndHasChanged(true);
                }
                if (CurrentPathNode.Flags2.Value != flags2)
                {
                    CurrentPathNode.Flags2 = (byte)flags2;
                    ProjectForm.SetYndHasChanged(true);
                }
                if (CurrentPathNode.Flags3.Value != flags3)
                {
                    CurrentPathNode.Flags3 = (byte)flags3;
                    ProjectForm.SetYndHasChanged(true);
                }
                if (CurrentPathNode.Flags4.Value != flags4)
                {
                    CurrentPathNode.Flags4 = (byte)flags4;
                    ProjectForm.SetYndHasChanged(true);
                }
                if (CurrentPathNode.LinkCountUnk != flags5)
                {
                    CurrentPathNode.LinkCountUnk = (byte)flags5;
                    ProjectForm.SetYndHasChanged(true);
                }
            }

            populatingui = true;
            UpdatePathNodeFlagsUI(true, false); //update checkboxes
            populatingui = false;
        }

        private void SetPathNodeLinkFlagsFromCheckBoxes()
        {
            if (populatingui) return;
            if (CurrentPathLink == null) return;

            uint flags0 = 0;
            uint flags1 = 0;
            uint flags2 = 0;
            flags0 = BitUtil.UpdateBit(flags0, 0, PathNodeLinkFlags01CheckBox.Checked);
            flags0 = BitUtil.UpdateBit(flags0, 1, PathNodeLinkFlags02CheckBox.Checked);
            flags0 += (((uint)PathNodeLinkFlags03UpDown.Value & 7u) << 2);
            flags0 += (((uint)PathNodeLinkFlags04UpDown.Value & 7u) << 5);

            flags1 = BitUtil.UpdateBit(flags1, 0, PathNodeLinkFlags11CheckBox.Checked);
            flags1 = BitUtil.UpdateBit(flags1, 1, PathNodeLinkFlags12CheckBox.Checked);
            flags1 = BitUtil.UpdateBit(flags1, 2, PathNodeLinkFlags13CheckBox.Checked);
            flags1 = BitUtil.UpdateBit(flags1, 3, PathNodeLinkFlags14CheckBox.Checked);
            flags1 += (((uint)PathNodeLinkOffsetSizeUpDown.Value & 7u) << 4);
            flags1 = BitUtil.UpdateBit(flags1, 7, PathNodeLinkFlags18CheckBox.Checked);

            flags2 = BitUtil.UpdateBit(flags2, 0, PathNodeLinkFlags21CheckBox.Checked);
            flags2 = BitUtil.UpdateBit(flags2, 1, PathNodeLinkFlags22CheckBox.Checked);
            flags2 += (((uint)PathNodeLinkBackLanesUpDown.Value & 7u) << 2);
            flags2 += (((uint)PathNodeLinkFwdLanesUpDown.Value & 7u) << 5);

            bool updgfx = false;
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CurrentPathLink.Flags0.Value != flags0)
                {
                    CurrentPathLink.Flags0 = (byte)flags0;
                    ProjectForm.SetYndHasChanged(true);
                }
                if (CurrentPathLink.Flags1.Value != flags1)
                {
                    CurrentPathLink.Flags1 = (byte)flags1;
                    ProjectForm.SetYndHasChanged(true);
                    updgfx = true;
                }
                if (CurrentPathLink.Flags2.Value != flags2)
                {
                    CurrentPathLink.Flags2 = (byte)flags2;
                    ProjectForm.SetYndHasChanged(true);
                    updgfx = true;
                }
            }

            populatingui = true;
            UpdatePathNodeLinkFlagsUI(false, true); //update updowns
            populatingui = false;

            if (updgfx && (ProjectForm.WorldForm != null) && (CurrentYndFile != null))
            {
                ProjectForm.WorldForm.UpdatePathYndGraphics(CurrentYndFile, false);
            }
        }

        private void SetPathNodeLinkFlagsFromUpDowns()
        {
            if (populatingui) return;
            if (CurrentPathLink == null) return;

            uint flags0 = (uint)PathNodeLinkFlags0UpDown.Value;
            uint flags1 = (uint)PathNodeLinkFlags1UpDown.Value;
            uint flags2 = (uint)PathNodeLinkFlags2UpDown.Value;

            bool updgfx = false;
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CurrentPathLink.Flags0.Value != flags0)
                {
                    CurrentPathLink.Flags0 = (byte)flags0;
                    ProjectForm.SetYndHasChanged(true);
                }
                if (CurrentPathLink.Flags1.Value != flags1)
                {
                    CurrentPathLink.Flags1 = (byte)flags1;
                    ProjectForm.SetYndHasChanged(true);
                }
                if (CurrentPathLink.Flags2.Value != flags2)
                {
                    CurrentPathLink.Flags2 = (byte)flags2;
                    ProjectForm.SetYndHasChanged(true);
                    updgfx = true;
                }
            }

            populatingui = true;
            UpdatePathNodeLinkFlagsUI(true, false); //update checkboxes
            populatingui = false;

            if (updgfx && (ProjectForm.WorldForm != null) && (CurrentYndFile != null))
            {
                ProjectForm.WorldForm.UpdatePathYndGraphics(CurrentYndFile, false);
            }
        }


        private void AddPathLink()
        {
            if (CurrentPathNode == null) return;

            var l = CurrentPathNode.AddLink();

            LoadPathNodeTabPage();

            PathNodeLinksListBox.SelectedItem = l;

            if (ProjectForm.WorldForm != null)
            {
                ProjectForm.WorldForm.UpdatePathNodeGraphics(CurrentPathNode, false);
            }
        }

        private void RemovePathLink()
        {
            if (CurrentPathLink == null) return;
            if (CurrentPathNode == null) return;

            var r = CurrentPathNode.RemoveLink(CurrentPathLink);

            if (!r) return;

            LoadPathNodeTabPage();

            if (ProjectForm.WorldForm != null)
            {
                ProjectForm.WorldForm.UpdatePathNodeGraphics(CurrentPathNode, false);
            }
        }

        private void UpdatePathNodeLinkage()
        {
            if (CurrentPathLink == null) return;
            if (CurrentYndFile == null) return;

            YndNode linknode = null;
            ushort areaid = CurrentPathLink._RawData.AreaID;
            ushort nodeid = CurrentPathLink._RawData.NodeID;
            if (areaid == CurrentYndFile.AreaID)
            {
                //link to the same ynd. find the new node in the current ynd.
                if ((CurrentYndFile.Nodes != null) && (nodeid < CurrentYndFile.Nodes.Length))
                {
                    linknode = CurrentYndFile.Nodes[nodeid];
                }
            }
            else
            {
                //try lookup the link node from the space.
                if (ProjectForm.WorldForm != null)
                {
                    linknode = ProjectForm.WorldForm.GetPathNodeFromSpace(areaid, nodeid);
                }
            }

            if (linknode == null)
            {
                PathNodeLinkageStatusLabel.Text = "Unable to find node " + areaid.ToString() + ":" + nodeid.ToString() + ".";
            }
            else
            {
                PathNodeLinkageStatusLabel.Text = "";
            }

            CurrentPathLink.Node2 = linknode;
            CurrentPathLink.UpdateLength();


            ////need to rebuild the link verts.. updating the graphics should do it...
            if (ProjectForm.WorldForm != null)
            {
                ProjectForm.WorldForm.UpdatePathYndGraphics(CurrentYndFile, false);
            }
        }







        private void PathNodeAreaIDUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentPathNode == null) return;
            ushort areaid = (ushort)PathNodeAreaIDUpDown.Value;
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CurrentPathNode.AreaID != areaid)
                {
                    CurrentPathNode.AreaID = areaid;
                    ProjectForm.SetYndHasChanged(true);
                }
            }

            ProjectForm.ProjectExplorer?.UpdatePathNodeTreeNode(CurrentPathNode);
        }

        private void PathNodeNodeIDUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentPathNode == null) return;
            ushort nodeid = (ushort)PathNodeNodeIDUpDown.Value;
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CurrentPathNode.NodeID != nodeid)
                {
                    CurrentPathNode.NodeID = nodeid;
                    ProjectForm.SetYndHasChanged(true);
                }
            }

            ProjectForm.ProjectExplorer?.UpdatePathNodeTreeNode(CurrentPathNode);
        }

        private void PathNodePositionTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentPathNode == null) return;
            Vector3 v = FloatUtil.ParseVector3String(PathNodePositionTextBox.Text);
            bool change = false;
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CurrentPathNode.Position != v)
                {
                    CurrentPathNode.SetPosition(v);
                    ProjectForm.SetYndHasChanged(true);
                    change = true;
                }
            }
            if (change)
            {
                if (ProjectForm.WorldForm != null)
                {
                    ProjectForm.WorldForm.SetWidgetPosition(CurrentPathNode.Position);
                    ProjectForm.WorldForm.UpdatePathNodeGraphics(CurrentPathNode, false);
                }
                //PathNodePositionTextBox.Text = FloatUtil.GetVector3String(CurrentPathNode.Position);
            }
        }

        private void PathNodeGoToButton_Click(object sender, EventArgs e)
        {
            if (CurrentPathNode == null) return;
            if (ProjectForm.WorldForm == null) return;
            ProjectForm.WorldForm.GoToPosition(CurrentPathNode.Position);
        }

        private void PathNodeAddToProjectButton_Click(object sender, EventArgs e)
        {
            if (CurrentPathNode?.Ynd != null)
            {
                ProjectForm.AddYndToProject(CurrentPathNode.Ynd);

            }
        }

        private void PathNodeDeleteButton_Click(object sender, EventArgs e)
        {
            ProjectForm.SetProjectItem(CurrentPathNode);
            ProjectForm.DeletePathNode();
        }

        private void PathNodeStreetHashTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentPathNode == null) return;
            uint hash;
            uint.TryParse(PathNodeStreetHashTextBox.Text, out hash);
            var streetname = GlobalText.TryGetString(hash);
            PathNodeStreetNameLabel.Text = "Name: " + ((hash == 0) ? "[None]" : (string.IsNullOrEmpty(streetname) ? "[Not found]" : streetname));

            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CurrentPathNode.StreetName.Hash != hash)
                {
                    CurrentPathNode.StreetName = hash;
                    ProjectForm.SetYndHasChanged(true);
                }
            }

            ProjectForm.ProjectExplorer?.UpdatePathNodeTreeNode(CurrentPathNode);
        }

        private void PathNodeFlags0UpDown_ValueChanged(object sender, EventArgs e)
        {
            SetPathNodeFlagsFromUpDowns();
        }

        private void PathNodeFlags1UpDown_ValueChanged(object sender, EventArgs e)
        {
            SetPathNodeFlagsFromUpDowns();
        }

        private void PathNodeFlags2UpDown_ValueChanged(object sender, EventArgs e)
        {
            SetPathNodeFlagsFromUpDowns();
        }

        private void PathNodeFlags3UpDown_ValueChanged(object sender, EventArgs e)
        {
            SetPathNodeFlagsFromUpDowns();
        }

        private void PathNodeFlags4UpDown_ValueChanged(object sender, EventArgs e)
        {
            SetPathNodeFlagsFromUpDowns();
        }

        private void PathNodeFlags5UpDown_ValueChanged(object sender, EventArgs e)
        {
            SetPathNodeFlagsFromUpDowns();
        }

        private void PathNodeFlags01CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            SetPathNodeFlagsFromCheckBoxes();
        }

        private void PathNodeFlags02CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            SetPathNodeFlagsFromCheckBoxes();
        }

        private void PathNodeFlags03CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            SetPathNodeFlagsFromCheckBoxes();
        }

        private void PathNodeFlags04CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            SetPathNodeFlagsFromCheckBoxes();
        }

        private void PathNodeFlags05CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            SetPathNodeFlagsFromCheckBoxes();
        }

        private void PathNodeFlags06CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            SetPathNodeFlagsFromCheckBoxes();
        }

        private void PathNodeFlags07CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            SetPathNodeFlagsFromCheckBoxes();
        }

        private void PathNodeFlags08CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            SetPathNodeFlagsFromCheckBoxes();
        }

        private void PathNodeFlags11CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            SetPathNodeFlagsFromCheckBoxes();
        }

        private void PathNodeFlags12CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            SetPathNodeFlagsFromCheckBoxes();
        }

        private void PathNodeFlags13CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            SetPathNodeFlagsFromCheckBoxes();
        }

        private void PathNodeFlags14CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            SetPathNodeFlagsFromCheckBoxes();
        }

        private void PathNodeFlags15CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            SetPathNodeFlagsFromCheckBoxes();
        }

        private void PathNodeFlags16CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            SetPathNodeFlagsFromCheckBoxes();
        }

        private void PathNodeFlags17CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            SetPathNodeFlagsFromCheckBoxes();
        }

        private void PathNodeFlags18CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            SetPathNodeFlagsFromCheckBoxes();
        }

        private void PathNodeFlags21CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            SetPathNodeFlagsFromCheckBoxes();
        }

        private void PathNodeFlags22CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            SetPathNodeFlagsFromCheckBoxes();
        }

        private void PathNodeFlags23CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            SetPathNodeFlagsFromCheckBoxes();
        }

        private void PathNodeFlags24CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            SetPathNodeFlagsFromCheckBoxes();
        }

        private void PathNodeFlags25CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            SetPathNodeFlagsFromCheckBoxes();
        }

        private void PathNodeFlags26CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            SetPathNodeFlagsFromCheckBoxes();
        }

        private void PathNodeFlags27CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            SetPathNodeFlagsFromCheckBoxes();
        }

        private void PathNodeFlags28CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            SetPathNodeFlagsFromCheckBoxes();
        }

        private void PathNodeFlags31CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            SetPathNodeFlagsFromCheckBoxes();
        }

        private void PathNodeFlags32UpDown_ValueChanged(object sender, EventArgs e)
        {
            SetPathNodeFlagsFromCheckBoxes(); //treat this one like checkboxes
        }

        private void PathNodeFlags51CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            SetPathNodeFlagsFromCheckBoxes();
        }

        private void PathNodeFlags41CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            SetPathNodeFlagsFromCheckBoxes();
        }

        private void PathNodeFlags45CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            SetPathNodeFlagsFromCheckBoxes();
        }

        private void PathNodeFlags46CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            SetPathNodeFlagsFromCheckBoxes();
        }

        private void PathNodeFlags47CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            SetPathNodeFlagsFromCheckBoxes();
        }

        private void PathNodeFlags48CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            SetPathNodeFlagsFromCheckBoxes();
        }

        private void PathNodeFlags42UpDown_ValueChanged(object sender, EventArgs e)
        {
            SetPathNodeFlagsFromCheckBoxes(); //treat this one like checkboxes
        }

        private void PathNodeFlags52CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            SetPathNodeFlagsFromCheckBoxes();
        }

        private void PathNodeFlags53CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            SetPathNodeFlagsFromCheckBoxes();
        }


        private void PathNodeLinksListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            CurrentPathLink = PathNodeLinksListBox.SelectedItem as YndLink;
            LoadPathNodeLinkPage();
        }

        private void PathNodeAddLinkButton_Click(object sender, EventArgs e)
        {
            AddPathLink();
        }

        private void PathNodeRemoveLinkButton_Click(object sender, EventArgs e)
        {
            RemovePathLink();
        }

        private void PathNodeLinkAreaIDUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentPathLink == null) return;
            ushort areaid = (ushort)PathNodeLinkAreaIDUpDown.Value;
            bool change = false;
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CurrentPathLink._RawData.AreaID != areaid)
                {
                    CurrentPathLink._RawData.AreaID = areaid;
                    ProjectForm.SetYndHasChanged(true);
                    change = true;
                }
            }
            if (change)
            {
                UpdatePathNodeLinkage();

                //refresh the selected item in the list box, to update the text, and the other controls.
                PathNodeLinksListBox.Items[PathNodeLinksListBox.SelectedIndex] = PathNodeLinksListBox.SelectedItem;
            }
        }

        private void PathNodeLinkNodeIDUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentPathLink == null) return;
            ushort nodeid = (ushort)PathNodeLinkNodeIDUpDown.Value;
            bool change = false;
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CurrentPathLink._RawData.NodeID != nodeid)
                {
                    CurrentPathLink._RawData.NodeID = nodeid;
                    ProjectForm.SetYndHasChanged(true);
                    change = true;
                }
            }
            if (change)
            {
                UpdatePathNodeLinkage();

                //refresh the selected item in the list box, to update the text, and the other controls.
                PathNodeLinksListBox.Items[PathNodeLinksListBox.SelectedIndex] = PathNodeLinksListBox.SelectedItem;
            }
        }

        private void PathNodeLinkFlags0UpDown_ValueChanged(object sender, EventArgs e)
        {
            SetPathNodeLinkFlagsFromUpDowns();
        }

        private void PathNodeLinkFlags1UpDown_ValueChanged(object sender, EventArgs e)
        {
            SetPathNodeLinkFlagsFromUpDowns();
        }

        private void PathNodeLinkFlags2UpDown_ValueChanged(object sender, EventArgs e)
        {
            SetPathNodeLinkFlagsFromUpDowns();
        }

        private void PathNodeLinkFlags01CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            SetPathNodeLinkFlagsFromCheckBoxes();
        }

        private void PathNodeLinkFlags02CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            SetPathNodeLinkFlagsFromCheckBoxes();
        }

        private void PathNodeLinkFlags03UpDown_ValueChanged(object sender, EventArgs e)
        {
            SetPathNodeLinkFlagsFromCheckBoxes();
        }

        private void PathNodeLinkFlags04UpDown_ValueChanged(object sender, EventArgs e)
        {
            SetPathNodeLinkFlagsFromCheckBoxes();
        }

        private void PathNodeLinkFlags11CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            SetPathNodeLinkFlagsFromCheckBoxes();
        }

        private void PathNodeLinkFlags12CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            SetPathNodeLinkFlagsFromCheckBoxes();
        }

        private void PathNodeLinkFlags13CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            SetPathNodeLinkFlagsFromCheckBoxes();
        }

        private void PathNodeLinkFlags14CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            SetPathNodeLinkFlagsFromCheckBoxes();
        }

        private void PathNodeLinkFlags18CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            SetPathNodeLinkFlagsFromCheckBoxes();
        }

        private void PathNodeLinkOffsetSizeUpDown_ValueChanged(object sender, EventArgs e)
        {
            SetPathNodeLinkFlagsFromCheckBoxes();
        }

        private void PathNodeLinkFlags21CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            SetPathNodeLinkFlagsFromCheckBoxes();
        }

        private void PathNodeLinkFlags22CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            SetPathNodeLinkFlagsFromCheckBoxes();
        }

        private void PathNodeLinkFwdLanesUpDown_ValueChanged(object sender, EventArgs e)
        {
            SetPathNodeLinkFlagsFromCheckBoxes();
        }

        private void PathNodeLinkBackLanesUpDown_ValueChanged(object sender, EventArgs e)
        {
            SetPathNodeLinkFlagsFromCheckBoxes();
        }

        private void PathNodeLinkLengthUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentPathLink == null) return;
            byte length = (byte)PathNodeLinkLengthUpDown.Value;
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CurrentPathLink.LinkLength.Value != length)
                {
                    CurrentPathLink.LinkLength = length;
                    CurrentPathLink._RawData.LinkLength = length;
                    ProjectForm.SetYndHasChanged(true);
                }
            }
        }


        private void PathNodeJunctionEnableCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentPathNode == null) return;
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CurrentPathNode.HasJunction != PathNodeJunctionEnableCheckBox.Checked)
                {
                    CurrentPathNode.HasJunction = PathNodeJunctionEnableCheckBox.Checked;
                    if (CurrentPathNode.HasJunction && (CurrentPathNode.Junction == null))
                    {
                        var j = new YndJunction();
                        //init new junction
                        j._RawData.HeightmapDimX = 1;
                        j._RawData.HeightmapDimY = 1;
                        j.Heightmap = new YndJunctionHeightmap(new byte[] { 255 }, j);
                        j.RefData = new NodeJunctionRef() { AreaID = (ushort)CurrentPathNode.AreaID, NodeID = (ushort)CurrentPathNode.NodeID };

                        CurrentPathNode.Junction = j;
                    }
                    ProjectForm.SetYndHasChanged(true);
                }
            }
            LoadPathNodeJunctionPage();
        }

        private void PathNodeJunctionMaxZUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentPathNode == null) return;
            if (CurrentPathNode.Junction == null) return;
            short val = (short)PathNodeJunctionMaxZUpDown.Value;
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CurrentPathNode.Junction.MaxZ != val)
                {
                    CurrentPathNode.Junction.MaxZ = val;
                    CurrentPathNode.Junction._RawData.MaxZ = val;
                    ProjectForm.SetYndHasChanged(true);
                }
            }
        }

        private void PathNodeJunctionMinZUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentPathNode == null) return;
            if (CurrentPathNode.Junction == null) return;
            short val = (short)PathNodeJunctionMinZUpDown.Value;
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CurrentPathNode.Junction.MinZ != val)
                {
                    CurrentPathNode.Junction.MinZ = val;
                    CurrentPathNode.Junction._RawData.MinZ = val;
                    ProjectForm.SetYndHasChanged(true);
                }
            }
        }

        private void PathNodeJunctionPosXUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentPathNode == null) return;
            if (CurrentPathNode.Junction == null) return;
            short val = (short)PathNodeJunctionPosXUpDown.Value;
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CurrentPathNode.Junction.PositionX != val)
                {
                    CurrentPathNode.Junction.PositionX = val;
                    CurrentPathNode.Junction._RawData.PositionX = val;
                    ProjectForm.SetYndHasChanged(true);
                }
            }
        }

        private void PathNodeJunctionPosYUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentPathNode == null) return;
            if (CurrentPathNode.Junction == null) return;
            short val = (short)PathNodeJunctionPosYUpDown.Value;
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CurrentPathNode.Junction.PositionY != val)
                {
                    CurrentPathNode.Junction.PositionY = val;
                    CurrentPathNode.Junction._RawData.PositionY = val;
                    ProjectForm.SetYndHasChanged(true);
                }
            }
        }

        private void PathNodeJunctionHeightmapDimXUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentPathNode == null) return;
            if (CurrentPathNode.Junction == null) return;
            byte val = (byte)PathNodeJunctionHeightmapDimXUpDown.Value;
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CurrentPathNode.Junction._RawData.HeightmapDimX != val)
                {
                    CurrentPathNode.Junction._RawData.HeightmapDimX = val;
                    CurrentPathNode.Junction.ResizeHeightmap();
                    ProjectForm.SetYndHasChanged(true);
                }
            }
            LoadPathNodeJunctionPage();
        }

        private void PathNodeJunctionHeightmapDimYUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentPathNode == null) return;
            if (CurrentPathNode.Junction == null) return;
            byte val = (byte)PathNodeJunctionHeightmapDimYUpDown.Value;
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CurrentPathNode.Junction._RawData.HeightmapDimY != val)
                {
                    CurrentPathNode.Junction._RawData.HeightmapDimY = val;
                    CurrentPathNode.Junction.ResizeHeightmap();
                    ProjectForm.SetYndHasChanged(true);
                }
            }
            LoadPathNodeJunctionPage();
        }

        private void PathNodeJunctionHeightmapBytesTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentPathNode == null) return;
            if (CurrentPathNode.Junction == null) return;
            lock (ProjectForm.ProjectSyncRoot)
            {
                CurrentPathNode.Junction.SetHeightmap(PathNodeJunctionHeightmapBytesTextBox.Text);
                ProjectForm.SetYndHasChanged(true);
            }
            //LoadPathNodeJunctionPage();
        }
    }
}
