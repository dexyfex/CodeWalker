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
    public partial class EditScenarioNodePanel : ProjectPanel
    {
        public ProjectForm ProjectForm;
        public ScenarioNode CurrentScenarioNode { get; set; }
        public YmtFile CurrentScenario { get; set; }
        public MCScenarioChainingEdge CurrentScenarioChainEdge { get; set; }

        private bool populatingui = false;

        public EditScenarioNodePanel(ProjectForm projectForm)
        {
            ProjectForm = projectForm;
            InitializeComponent();
        }

        public void SetScenarioNode(ScenarioNode node)
        {
            CurrentScenarioNode = node;
            CurrentScenario = node?.Ymt;
            Tag = node;
            UpdateFormTitle();
            UpdateScenarioNodeUI();
        }

        private void UpdateFormTitle()
        {
            var sn = CurrentScenarioNode.ToString();
            Text = sn;
        }

        public void UpdateScenarioNodeUI()
        {
            populatingui = true;

            LoadScenarioDropDowns();

            LoadScenarioPointTabPage();
            LoadScenarioEntityTabPage();
            LoadScenarioEntityPointTabPage();
            LoadScenarioChainTabPage();
            LoadScenarioChainEdgeTabPage();
            LoadScenarioChainNodeTabPage();
            LoadScenarioClusterTabPage();
            LoadScenarioClusterPointTabPage();
            populatingui = false;

            UpdateTabVisibility();

            if (CurrentScenarioNode != null)
            {
                if (ProjectForm.WorldForm != null)
                {
                    ProjectForm.WorldForm.SelectScenarioNode(CurrentScenarioNode);
                }
            }
        }

        private void UpdateTabVisibility()
        {


            //avoid resetting the tabs if no change is necessary.
            bool ok = true;
            bool pointTabVis = false;
            bool entTabVis = false;
            bool entPntTabVis = false;
            bool chainTabVis = false;
            bool chainNodeTabVis = false;
            bool clusterTabVis = false;
            bool clusterNodeTabVis = false;
            foreach (var tab in ScenarioTabControl.TabPages)
            {
                if (tab == ScenarioPointTabPage) pointTabVis = true;
                if (tab == ScenarioEntityTabPage) entTabVis = true;
                if (tab == ScenarioEntityPointTabPage) entPntTabVis = true;
                if (tab == ScenarioChainTabPage) chainTabVis = true;
                if (tab == ScenarioChainNodeTabPage) chainNodeTabVis = true;
                if (tab == ScenarioClusterTabPage) clusterTabVis = true;
                if (tab == ScenarioClusterPointTabPage) clusterNodeTabVis = true;
            }

            if ((CurrentScenarioNode?.MyPoint != null) != pointTabVis) ok = false;
            if ((CurrentScenarioNode?.Entity != null) != entTabVis) ok = false;
            if ((CurrentScenarioNode?.EntityPoint != null) != entPntTabVis) ok = false;
            if ((CurrentScenarioNode?.ChainingNode != null) != chainTabVis) ok = false;
            if ((CurrentScenarioNode?.ChainingNode != null) != chainNodeTabVis) ok = false;
            if ((CurrentScenarioNode?.Cluster != null) != clusterTabVis) ok = false;
            if ((CurrentScenarioNode?.ClusterMyPoint != null) != clusterNodeTabVis) ok = false;
            if (ok) return;

            var seltab = ScenarioTabControl.SelectedTab;

            ScenarioTabControl.TabPages.Clear();

            if (CurrentScenarioNode?.MyPoint != null) ScenarioTabControl.TabPages.Add(ScenarioPointTabPage);
            if (CurrentScenarioNode?.Entity != null) ScenarioTabControl.TabPages.Add(ScenarioEntityTabPage);
            if (CurrentScenarioNode?.EntityPoint != null) ScenarioTabControl.TabPages.Add(ScenarioEntityPointTabPage);
            if (CurrentScenarioNode?.ChainingNode != null) ScenarioTabControl.TabPages.Add(ScenarioChainTabPage);
            if (CurrentScenarioNode?.ChainingNode != null) ScenarioTabControl.TabPages.Add(ScenarioChainNodeTabPage);
            if (CurrentScenarioNode?.Cluster != null) ScenarioTabControl.TabPages.Add(ScenarioClusterTabPage);
            if (CurrentScenarioNode?.ClusterMyPoint != null) ScenarioTabControl.TabPages.Add(ScenarioClusterPointTabPage);

            if (ScenarioTabControl.TabPages.Contains(seltab))
            {
                ScenarioTabControl.SelectedTab = seltab;
            }
        }


        private void LoadScenarioDropDowns()
        {
            if (ScenarioPointTypeComboBox.Items.Count > 0) return;

            var types = Scenarios.ScenarioTypes; //these are loaded by Scenarios.Init
            if (types == null)
            { return; }

            var stypes = types.GetScenarioTypes();
            if (stypes == null) return;

            var pmsets = types.GetPedModelSets();
            if (pmsets == null) return;

            var vmsets = types.GetVehicleModelSets();
            if (vmsets == null) return;

            ScenarioPointTypeComboBox.Items.Clear();
            ScenarioPointTypeComboBox.Items.Add("");
            ScenarioClusterPointTypeComboBox.Items.Clear();
            ScenarioClusterPointTypeComboBox.Items.Add("");
            ScenarioChainNodeTypeComboBox.Items.Clear();
            ScenarioChainNodeTypeComboBox.Items.Add("");
            foreach (var stype in stypes)
            {
                ScenarioPointTypeComboBox.Items.Add(stype);
                ScenarioClusterPointTypeComboBox.Items.Add(stype);
                ScenarioChainNodeTypeComboBox.Items.Add(stype);
            }

            ScenarioPointModelSetComboBox.Items.Clear();
            ScenarioPointModelSetComboBox.Items.Add("");
            ScenarioClusterPointModelSetComboBox.Items.Clear();
            ScenarioClusterPointModelSetComboBox.Items.Add("");
            foreach (var pmset in pmsets)
            {
                ScenarioPointModelSetComboBox.Items.Add(pmset);
                ScenarioClusterPointModelSetComboBox.Items.Add(pmset);
            }
            foreach (var vmset in vmsets)
            {
                ScenarioPointModelSetComboBox.Items.Add(vmset);
                ScenarioClusterPointModelSetComboBox.Items.Add(vmset);
            }


            ScenarioEntityPointAvailableInMpSpComboBox.Items.Clear();
            ScenarioEntityPointAvailableInMpSpComboBox.Items.Add(Unk_3573596290.kBoth);
            ScenarioEntityPointAvailableInMpSpComboBox.Items.Add(Unk_3573596290.kOnlySp);
            ScenarioEntityPointAvailableInMpSpComboBox.Items.Add(Unk_3573596290.kOnlyMp);


            ScenarioChainEdgeActionComboBox.Items.Clear();
            ScenarioChainEdgeActionComboBox.Items.Add(Unk_3609807418.Move);
            ScenarioChainEdgeActionComboBox.Items.Add(Unk_3609807418.Unk_7865678);
            ScenarioChainEdgeActionComboBox.Items.Add(Unk_3609807418.MoveFollowMaster);

            ScenarioChainEdgeNavModeComboBox.Items.Clear();
            ScenarioChainEdgeNavModeComboBox.Items.Add(Unk_3971773454.Direct);
            ScenarioChainEdgeNavModeComboBox.Items.Add(Unk_3971773454.NavMesh);
            ScenarioChainEdgeNavModeComboBox.Items.Add(Unk_3971773454.Roads);

            ScenarioChainEdgeNavSpeedComboBox.Items.Clear();
            ScenarioChainEdgeNavSpeedComboBox.Items.Add(Unk_941086046.Unk_00_3279574318);
            ScenarioChainEdgeNavSpeedComboBox.Items.Add(Unk_941086046.Unk_01_2212923970);
            ScenarioChainEdgeNavSpeedComboBox.Items.Add(Unk_941086046.Unk_02_4022799658);
            ScenarioChainEdgeNavSpeedComboBox.Items.Add(Unk_941086046.Unk_03_1425672334);
            ScenarioChainEdgeNavSpeedComboBox.Items.Add(Unk_941086046.Unk_04_957720931);
            ScenarioChainEdgeNavSpeedComboBox.Items.Add(Unk_941086046.Unk_05_3795195414);
            ScenarioChainEdgeNavSpeedComboBox.Items.Add(Unk_941086046.Unk_06_2834622009);
            ScenarioChainEdgeNavSpeedComboBox.Items.Add(Unk_941086046.Unk_07_1876554076);
            ScenarioChainEdgeNavSpeedComboBox.Items.Add(Unk_941086046.Unk_08_698543797);
            ScenarioChainEdgeNavSpeedComboBox.Items.Add(Unk_941086046.Unk_09_1544199634);
            ScenarioChainEdgeNavSpeedComboBox.Items.Add(Unk_941086046.Unk_10_2725613303);
            ScenarioChainEdgeNavSpeedComboBox.Items.Add(Unk_941086046.Unk_11_4033265820);
            ScenarioChainEdgeNavSpeedComboBox.Items.Add(Unk_941086046.Unk_12_3054809929);
            ScenarioChainEdgeNavSpeedComboBox.Items.Add(Unk_941086046.Unk_13_3911005380);
            ScenarioChainEdgeNavSpeedComboBox.Items.Add(Unk_941086046.Unk_14_3717649022);
            ScenarioChainEdgeNavSpeedComboBox.Items.Add(Unk_941086046.Unk_15_3356026130);

        }

        private void LoadScenarioPointTabPage()
        {
            var p = CurrentScenarioNode?.MyPoint;
            if (p == null)
            {
                //ScenarioPointPanel.Enabled = false;
                //ScenarioPointCheckBox.Checked = false;
                ScenarioPointAddToProjectButton.Enabled = false;
                ScenarioPointDeleteButton.Enabled = false;
                ScenarioPointPositionTextBox.Text = "";
                ScenarioPointDirectionTextBox.Text = "";
                ScenarioPointTypeComboBox.SelectedItem = null;
                ScenarioPointModelSetComboBox.SelectedItem = null;
                ScenarioPointInteriorTextBox.Text = "";
                ScenarioPointInteriorHashLabel.Text = "Hash: 0";
                ScenarioPointGroupTextBox.Text = "";
                ScenarioPointGroupHashLabel.Text = "Hash: 0";
                ScenarioPointImapTextBox.Text = "";
                ScenarioPointImapHashLabel.Text = "Hash: 0";
                ScenarioPointTimeStartUpDown.Value = 0;
                ScenarioPointTimeEndUpDown.Value = 0;
                ScenarioPointProbabilityUpDown.Value = 0;
                ScenarioPointSpOnlyFlagUpDown.Value = 0;
                ScenarioPointRadiusUpDown.Value = 0;
                ScenarioPointWaitTimeUpDown.Value = 0;
                ScenarioPointFlagsValueUpDown.Value = 0;
                foreach (int i in ScenarioPointFlagsCheckedListBox.CheckedIndices)
                {
                    ScenarioPointFlagsCheckedListBox.SetItemCheckState(i, CheckState.Unchecked);
                }
            }
            else
            {
                //ScenarioPointPanel.Enabled = true;
                //ScenarioPointCheckBox.Checked = true;
                ScenarioPointDeleteButton.Enabled = ProjectForm.ScenarioExistsInProject(CurrentScenario);
                ScenarioPointAddToProjectButton.Enabled = !ScenarioPointDeleteButton.Enabled;
                ScenarioPointPositionTextBox.Text = FloatUtil.GetVector3String(p.Position);
                ScenarioPointDirectionTextBox.Text = FloatUtil.ToString(p.Direction);
                ScenarioPointTypeComboBox.SelectedItem = ((object)p.Type) ?? "";
                ScenarioPointModelSetComboBox.SelectedItem = ((object)p.ModelSet) ?? "";
                ScenarioPointInteriorTextBox.Text = p.InteriorName.ToString();
                ScenarioPointInteriorHashLabel.Text = "Hash: " + p.InteriorName.Hash.ToString();
                ScenarioPointGroupTextBox.Text = p.GroupName.ToString();
                ScenarioPointGroupHashLabel.Text = "Hash: " + p.GroupName.Hash.ToString();
                ScenarioPointImapTextBox.Text = p.IMapName.ToString();
                ScenarioPointImapHashLabel.Text = "Hash: " + p.IMapName.Hash.ToString();
                ScenarioPointTimeStartUpDown.Value = p.TimeStart;
                ScenarioPointTimeEndUpDown.Value = p.TimeEnd;
                ScenarioPointProbabilityUpDown.Value = p.Probability;
                ScenarioPointSpOnlyFlagUpDown.Value = p.AvailableMpSp;
                ScenarioPointRadiusUpDown.Value = p.Radius;
                ScenarioPointWaitTimeUpDown.Value = p.WaitTime;
                var iflags = (int)p.Flags;
                ScenarioPointFlagsValueUpDown.Value = iflags;
                for (int i = 0; i < ScenarioPointFlagsCheckedListBox.Items.Count; i++)
                {
                    var c = ((iflags & (1 << i)) > 0);
                    ScenarioPointFlagsCheckedListBox.SetItemCheckState(i, c ? CheckState.Checked : CheckState.Unchecked);
                }
            }
        }

        private void LoadScenarioEntityTabPage()
        {
            var e = CurrentScenarioNode?.Entity;
            if (e == null)
            {
                //ScenarioEntityPanel.Enabled = false;
                //ScenarioEntityCheckBox.Checked = false;
                ScenarioEntityAddToProjectButton.Enabled = false;
                ScenarioEntityDeleteButton.Enabled = false;
                ScenarioEntityPositionTextBox.Text = "";
                ScenarioEntityTypeTextBox.Text = "";
                ScenarioEntityTypeHashLabel.Text = "Hash: 0";
                ScenarioEntityUnk1UpDown.Value = 0;
                ScenarioEntityUnk2UpDown.Value = 0;
                ScenarioEntityInfoLabel.Text = "0 override points";
                ScenarioEntityPointsListBox.Items.Clear();
                ScenarioEntityAddPointButton.Enabled = false;
            }
            else
            {
                //ScenarioEntityPanel.Enabled = true;
                //ScenarioEntityCheckBox.Checked = true;
                ScenarioEntityDeleteButton.Enabled = ProjectForm.ScenarioExistsInProject(CurrentScenario);
                ScenarioEntityAddToProjectButton.Enabled = !ScenarioEntityDeleteButton.Enabled;
                ScenarioEntityPositionTextBox.Text = FloatUtil.GetVector3String(e.Position);
                ScenarioEntityTypeTextBox.Text = e.TypeName.ToString();
                ScenarioEntityTypeHashLabel.Text = "Hash: " + e.TypeName.Hash.ToString();
                ScenarioEntityUnk1UpDown.Value = e.Unk1;
                ScenarioEntityUnk2UpDown.Value = e.Unk2;
                var pc = e.ScenarioPoints?.Length ?? 0;
                ScenarioEntityInfoLabel.Text = pc.ToString() + " override point" + ((pc != 1) ? "s" : "");
                ScenarioEntityPointsListBox.Items.Clear();
                ScenarioEntityAddPointButton.Enabled = true;

                if (e.ScenarioPoints != null)
                {
                    foreach (var point in e.ScenarioPoints)
                    {
                        ScenarioEntityPointsListBox.Items.Add(point);
                    }
                    if (CurrentScenarioNode.EntityPoint != null)
                    {
                        ScenarioEntityPointsListBox.SelectedItem = CurrentScenarioNode.EntityPoint;
                    }
                }
            }
        }

        private void LoadScenarioEntityPointTabPage()
        {
            var p = CurrentScenarioNode?.EntityPoint;
            if (p == null)
            {
                //ScenarioEntityPointPanel.Enabled = false;
                //ScenarioEntityPointCheckBox.Checked = false;
                ScenarioEntityPointAddToProjectButton.Enabled = false;
                ScenarioEntityPointDeleteButton.Enabled = false;
                ScenarioEntityPointNameTextBox.Text = "";
                ScenarioEntityPointNameHashLabel.Text = "Hash: 0";
                ScenarioEntityPointPositionTextBox.Text = "";
                ScenarioEntityPointRotationTextBox.Text = "";
                ScenarioEntityPointSpawnTypeTextBox.Text = "";
                ScenarioEntityPointSpawnTypeHashLabel.Text = "Hash: 0";
                ScenarioEntityPointPedTypeTextBox.Text = "";
                ScenarioEntityPointPedTypeHashLabel.Text = "Hash: 0";
                ScenarioEntityPointGroupTextBox.Text = "";
                ScenarioEntityPointGroupHashLabel.Text = "Hash: 0";
                ScenarioEntityPointInteriorTextBox.Text = "";
                ScenarioEntityPointInteriorHashLabel.Text = "Hash: 0";
                ScenarioEntityPointRequiredImapTextBox.Text = "";
                ScenarioEntityPointRequiredImapHashLabel.Text = "Hash: 0";
                ScenarioEntityPointAvailableInMpSpComboBox.SelectedItem = null;
                ScenarioEntityPointProbabilityTextBox.Text = "";
                ScenarioEntityPointTimeTillPedLeavesTextBox.Text = "";
                ScenarioEntityPointRadiusTextBox.Text = "";
                ScenarioEntityPointStartUpDown.Value = 0;
                ScenarioEntityPointEndUpDown.Value = 0;
                ScenarioEntityPointExtendedRangeCheckBox.Checked = false;
                ScenarioEntityPointShortRangeCheckBox.Checked = false;
                ScenarioEntityPointHighPriCheckBox.Checked = false;
                ScenarioEntityPointFlagsUpDown.Value = 0;
                foreach (int i in ScenarioEntityPointFlagsCheckedListBox.CheckedIndices)
                {
                    ScenarioEntityPointFlagsCheckedListBox.SetItemCheckState(i, CheckState.Unchecked);
                }
            }
            else
            {
                //ScenarioEntityPointPanel.Enabled = true;
                //ScenarioEntityPointCheckBox.Checked = true;
                ScenarioEntityPointDeleteButton.Enabled = ProjectForm.ScenarioExistsInProject(CurrentScenario);
                ScenarioEntityPointAddToProjectButton.Enabled = !ScenarioEntityPointDeleteButton.Enabled;
                ScenarioEntityPointNameTextBox.Text = p.NameHash.ToString();
                ScenarioEntityPointNameHashLabel.Text = "Hash: " + p.NameHash.Hash.ToString();
                ScenarioEntityPointPositionTextBox.Text = FloatUtil.GetVector3String(p.OffsetPosition);
                ScenarioEntityPointRotationTextBox.Text = FloatUtil.GetVector4String(p.OffsetRotation);
                ScenarioEntityPointSpawnTypeTextBox.Text = p.SpawnType.ToString();
                ScenarioEntityPointSpawnTypeHashLabel.Text = "Hash: " + p.SpawnType.Hash.ToString();
                ScenarioEntityPointPedTypeTextBox.Text = p.PedType.ToString();
                ScenarioEntityPointPedTypeHashLabel.Text = "Hash: " + p.PedType.Hash.ToString();
                ScenarioEntityPointGroupTextBox.Text = p.Group.ToString();
                ScenarioEntityPointGroupHashLabel.Text = "Hash: " + p.Group.Hash.ToString();
                ScenarioEntityPointInteriorTextBox.Text = p.Interior.ToString();
                ScenarioEntityPointInteriorHashLabel.Text = "Hash: " + p.Interior.Hash.ToString();
                ScenarioEntityPointRequiredImapTextBox.Text = p.RequiredImap.ToString();
                ScenarioEntityPointRequiredImapHashLabel.Text = "Hash: " + p.RequiredImap.Hash.ToString();
                ScenarioEntityPointAvailableInMpSpComboBox.SelectedItem = p.AvailableInMpSp;
                ScenarioEntityPointProbabilityTextBox.Text = FloatUtil.ToString(p.Probability);
                ScenarioEntityPointTimeTillPedLeavesTextBox.Text = FloatUtil.ToString(p.TimeTillPedLeaves);
                ScenarioEntityPointRadiusTextBox.Text = FloatUtil.ToString(p.Radius);
                ScenarioEntityPointStartUpDown.Value = p.StartTime;
                ScenarioEntityPointEndUpDown.Value = p.EndTime;
                ScenarioEntityPointExtendedRangeCheckBox.Checked = p.ExtendedRange;
                ScenarioEntityPointShortRangeCheckBox.Checked = p.ShortRange;
                ScenarioEntityPointHighPriCheckBox.Checked = p.HighPri;
                var iflags = (int)p.Flags;
                ScenarioEntityPointFlagsUpDown.Value = 0;
                for (int i = 0; i < ScenarioEntityPointFlagsCheckedListBox.Items.Count; i++)
                {
                    var c = ((iflags & (1 << i)) > 0);
                    ScenarioEntityPointFlagsCheckedListBox.SetItemCheckState(i, c ? CheckState.Checked : CheckState.Unchecked);
                }

            }
        }

        private void LoadScenarioChainTabPage()
        {
            CurrentScenarioChainEdge = null;
            ProjectForm.SetScenarioChainEdge(CurrentScenarioChainEdge);

            var n = CurrentScenarioNode?.ChainingNode;
            if (n == null)
            {
                ScenarioChainAddToProjectButton.Enabled = false;
                ScenarioChainDeleteButton.Enabled = false;
                ScenarioChainEdgesListBox.Items.Clear();
                ScenarioChainEdgeCountLabel.Text = "Edge Count: 0";
                ScenarioChainUnk1UpDown.Value = 0;
            }
            else
            {
                ScenarioChainDeleteButton.Enabled = ScenarioChainNodeDeleteButton.Enabled;// ScenarioExistsInProject(CurrentScenario);
                ScenarioChainAddToProjectButton.Enabled = !ScenarioChainDeleteButton.Enabled;
                ScenarioChainEdgesListBox.Items.Clear();
                ScenarioChainEdgeCountLabel.Text = "Edge Count: " + (n.Chain?.EdgeIds?.Length ?? 0).ToString();
                ScenarioChainUnk1UpDown.Value = n.Chain?.Unk1 ?? 0;

                if ((n.Chain != null) && (n.Chain.Edges != null))
                {
                    foreach (var edge in n.Chain.Edges)
                    {
                        ScenarioChainEdgesListBox.Items.Add(edge);
                    }
                }
                else
                { }
            }
        }

        private void LoadScenarioChainEdgeTabPage()
        {
            var e = CurrentScenarioChainEdge;
            if (e == null)
            {
                ScenarioChainEdgePanel.Enabled = false;
                ScenarioChainEdgeNodeIndexFromUpDown.Value = 0;
                ScenarioChainEdgeNodeIndexToUpDown.Value = 0;
                ScenarioChainEdgeActionComboBox.SelectedItem = null;
                ScenarioChainEdgeNavModeComboBox.SelectedItem = null;
                ScenarioChainEdgeNavSpeedComboBox.SelectedItem = null;
                ScenarioChainMoveEdgeDownButton.Enabled = false;
                ScenarioChainMoveEdgeUpButton.Enabled = false;
            }
            else
            {
                ScenarioChainEdgePanel.Enabled = true;
                ScenarioChainEdgeNodeIndexFromUpDown.Value = e.NodeIndexFrom;
                ScenarioChainEdgeNodeIndexToUpDown.Value = e.NodeIndexTo;
                ScenarioChainEdgeActionComboBox.SelectedItem = e.Action;
                ScenarioChainEdgeNavModeComboBox.SelectedItem = e.NavMode;
                ScenarioChainEdgeNavSpeedComboBox.SelectedItem = e.NavSpeed;
                ScenarioChainMoveEdgeDownButton.Enabled = true;
                ScenarioChainMoveEdgeUpButton.Enabled = true;

                if (ProjectForm.WorldForm != null)
                {
                    ProjectForm.WorldForm.SelectScenarioEdge(CurrentScenarioNode, e);
                }
            }
        }

        private void LoadScenarioChainNodeTabPage()
        {
            var n = CurrentScenarioNode?.ChainingNode;
            if (n == null)
            {
                //ScenarioChainNodePanel.Enabled = false;
                //ScenarioChainNodeCheckBox.Checked = false;
                ScenarioChainNodeAddToProjectButton.Enabled = false;
                ScenarioChainNodeDeleteButton.Enabled = false;
                ScenarioChainNodePositionTextBox.Text = "";
                ScenarioChainNodeUnk1TextBox.Text = "";
                ScenarioChainNodeUnk1HashLabel.Text = "Hash: 0";
                ScenarioChainNodeTypeComboBox.SelectedItem = null;
                ScenarioChainNodeFirstCheckBox.Checked = false;
                ScenarioChainNodeLastCheckBox.Checked = false;
                ScenarioChainNodeIndexTextBox.Text = "";
            }
            else
            {
                //ScenarioChainNodePanel.Enabled = true;
                //ScenarioChainNodeCheckBox.Checked = true;
                ScenarioChainNodeDeleteButton.Enabled = ProjectForm.ScenarioExistsInProject(CurrentScenario);
                ScenarioChainNodeAddToProjectButton.Enabled = !ScenarioChainNodeDeleteButton.Enabled;
                ScenarioChainNodePositionTextBox.Text = FloatUtil.GetVector3String(n.Position);
                ScenarioChainNodeUnk1TextBox.Text = n.Unk1.ToString();
                ScenarioChainNodeUnk1HashLabel.Text = "Hash: " + n.Unk1.Hash.ToString();
                ScenarioChainNodeTypeComboBox.SelectedItem = ((object)n.Type) ?? "";
                ScenarioChainNodeFirstCheckBox.Checked = !n.NotFirst;
                ScenarioChainNodeLastCheckBox.Checked = !n.NotLast;
                ScenarioChainNodeIndexTextBox.Text = n.NodeIndex.ToString();
            }
        }

        private void LoadScenarioClusterTabPage()
        {
            var c = CurrentScenarioNode?.Cluster;
            if (c == null)
            {
                //ScenarioClusterPanel.Enabled = false;
                //ScenarioClusterCheckBox.Checked = false;
                ScenarioClusterAddToProjectButton.Enabled = false;
                ScenarioClusterDeleteButton.Enabled = false;
                ScenarioClusterCenterTextBox.Text = "";
                ScenarioClusterRadiusTextBox.Text = "";
                ScenarioClusterUnk1TextBox.Text = "";
                ScenarioClusterUnk2CheckBox.Checked = false;
                ScenarioClusterPointsListBox.Items.Clear();
                ScenarioClusterAddPointButton.Enabled = false;
            }
            else
            {
                //ScenarioClusterPanel.Enabled = true;
                //ScenarioClusterCheckBox.Checked = true;
                ScenarioClusterDeleteButton.Enabled = ProjectForm.ScenarioExistsInProject(CurrentScenario);
                ScenarioClusterAddToProjectButton.Enabled = !ScenarioClusterDeleteButton.Enabled;
                ScenarioClusterCenterTextBox.Text = FloatUtil.GetVector3String(c.Position);
                ScenarioClusterRadiusTextBox.Text = FloatUtil.ToString(c.Radius);
                ScenarioClusterUnk1TextBox.Text = FloatUtil.ToString(c.Unk1);
                ScenarioClusterUnk2CheckBox.Checked = c.Unk2;
                ScenarioClusterPointsListBox.Items.Clear();
                ScenarioClusterAddPointButton.Enabled = true;

                if (c.Points != null)
                {
                    if (c.Points.MyPoints != null)
                    {
                        foreach (var point in c.Points.MyPoints)
                        {
                            ScenarioClusterPointsListBox.Items.Add(point);
                        }
                        if (CurrentScenarioNode.ClusterMyPoint != null)
                        {
                            ScenarioClusterPointsListBox.SelectedItem = CurrentScenarioNode.ClusterMyPoint;
                        }
                    }
                    if (c.Points.LoadSavePoints != null)
                    {
                        foreach (var point in c.Points.LoadSavePoints)
                        {
                            ScenarioClusterPointsListBox.Items.Add(point);
                        }
                        if (CurrentScenarioNode.ClusterLoadSavePoint != null)
                        {
                            ScenarioClusterPointsListBox.SelectedItem = CurrentScenarioNode.ClusterLoadSavePoint;
                        }
                    }
                }

            }
        }

        private void LoadScenarioClusterPointTabPage()
        {
            var p = CurrentScenarioNode?.ClusterMyPoint;
            if (p == null)
            {
                //ScenarioClusterPointPanel.Enabled = false;
                //ScenarioClusterPointCheckBox.Checked = false;
                ScenarioClusterPointAddToProjectButton.Enabled = false;
                ScenarioClusterPointDeleteButton.Enabled = false;
                ScenarioClusterPointPositionTextBox.Text = "";
                ScenarioClusterPointDirectionTextBox.Text = "";
                ScenarioClusterPointTypeComboBox.SelectedItem = null;
                ScenarioClusterPointModelSetComboBox.SelectedItem = null;
                ScenarioClusterPointInteriorTextBox.Text = "";
                ScenarioClusterPointInteriorHashLabel.Text = "Hash: 0";
                ScenarioClusterPointGroupTextBox.Text = "";
                ScenarioClusterPointGroupHashLabel.Text = "Hash: 0";
                ScenarioClusterPointImapTextBox.Text = "";
                ScenarioClusterPointImapHashLabel.Text = "Hash: 0";
                ScenarioClusterPointTimeStartUpDown.Value = 0;
                ScenarioClusterPointTimeEndUpDown.Value = 0;
                ScenarioClusterPointProbabilityUpDown.Value = 0;
                ScenarioClusterPointSpOnlyFlagUpDown.Value = 0;
                ScenarioClusterPointRadiusUpDown.Value = 0;
                ScenarioClusterPointWaitTimeUpDown.Value = 0;
                ScenarioClusterPointFlagsUpDown.Value = 0;
                foreach (int i in ScenarioClusterPointFlagsCheckedListBox.CheckedIndices)
                {
                    ScenarioClusterPointFlagsCheckedListBox.SetItemCheckState(i, CheckState.Unchecked);
                }
            }
            else
            {
                //ScenarioClusterPointPanel.Enabled = true;
                //ScenarioClusterPointCheckBox.Checked = true;
                ScenarioClusterPointDeleteButton.Enabled = ProjectForm.ScenarioExistsInProject(CurrentScenario);
                ScenarioClusterPointAddToProjectButton.Enabled = !ScenarioClusterPointDeleteButton.Enabled;
                ScenarioClusterPointPositionTextBox.Text = FloatUtil.GetVector3String(p.Position);
                ScenarioClusterPointDirectionTextBox.Text = FloatUtil.ToString(p.Direction);
                ScenarioClusterPointTypeComboBox.SelectedItem = ((object)p.Type) ?? "";
                ScenarioClusterPointModelSetComboBox.SelectedItem = ((object)p.ModelSet) ?? "";
                ScenarioClusterPointInteriorTextBox.Text = p.InteriorName.ToString();
                ScenarioClusterPointInteriorHashLabel.Text = "Hash: " + p.InteriorName.Hash.ToString();
                ScenarioClusterPointGroupTextBox.Text = p.GroupName.ToString();
                ScenarioClusterPointGroupHashLabel.Text = "Hash: " + p.GroupName.Hash.ToString();
                ScenarioClusterPointImapTextBox.Text = p.IMapName.ToString();
                ScenarioClusterPointImapHashLabel.Text = "Hash: " + p.IMapName.Hash.ToString();
                ScenarioClusterPointTimeStartUpDown.Value = p.TimeStart;
                ScenarioClusterPointTimeEndUpDown.Value = p.TimeEnd;
                ScenarioClusterPointProbabilityUpDown.Value = p.Probability;
                ScenarioClusterPointSpOnlyFlagUpDown.Value = p.AvailableMpSp;
                ScenarioClusterPointRadiusUpDown.Value = p.Radius;
                ScenarioClusterPointWaitTimeUpDown.Value = p.WaitTime;
                var iflags = (int)p.Flags;
                ScenarioClusterPointFlagsUpDown.Value = iflags;
                for (int i = 0; i < ScenarioClusterPointFlagsCheckedListBox.Items.Count; i++)
                {
                    var c = ((iflags & (1 << i)) > 0);
                    ScenarioClusterPointFlagsCheckedListBox.SetItemCheckState(i, c ? CheckState.Checked : CheckState.Unchecked);
                }
            }
        }









        private void AddScenarioChainEdge()
        {
            var chain = CurrentScenarioNode?.ChainingNode?.Chain;
            if (chain == null) return;
            var paths = CurrentScenario?.CScenarioPointRegion?.Paths;
            if (paths == null) return;

            MCScenarioChainingEdge edge = new MCScenarioChainingEdge();
            if (CurrentScenarioChainEdge != null)
            {
                edge.Data = CurrentScenarioChainEdge.Data;
            }

            paths.AddEdge(edge);
            chain.AddEdge(edge);

            CurrentScenarioChainEdge = edge;
            ProjectForm.SetScenarioChainEdge(CurrentScenarioChainEdge);

            UpdateScenarioChainEdgeLinkage();

            LoadScenarioChainTabPage();

            ScenarioChainEdgesListBox.SelectedItem = edge;
        }

        private void RemoveScenarioChainEdge()
        {
            if (CurrentScenarioChainEdge == null) return;
            if (CurrentScenario == null) return;

            var chain = CurrentScenarioNode?.ChainingNode?.Chain;
            if (chain == null) return;
            var paths = CurrentScenario?.CScenarioPointRegion?.Paths;
            if (paths == null) return;

            if (MessageBox.Show("Are you sure you want to delete this scenario chain edge?\n" + CurrentScenarioChainEdge.ToString() + "\n\nThis operation cannot be undone. Continue?", "Confirm delete", MessageBoxButtons.YesNo) != DialogResult.Yes)
            {
                return;
            }

            chain.RemoveEdge(CurrentScenarioChainEdge);
            paths.RemoveEdge(CurrentScenarioChainEdge);

            LoadScenarioChainTabPage();

            if (ProjectForm.WorldForm != null)
            {
                ProjectForm.WorldForm.UpdateScenarioGraphics(CurrentScenario, false);
                ProjectForm.WorldForm.SelectScenarioEdge(CurrentScenarioNode, null);
            }
        }

        private void MoveScenarioChainEdge(bool moveDown)
        {

            var chain = CurrentScenarioNode?.ChainingNode?.Chain;
            if (chain == null) return;
            if (chain.Edges == null) return;
            if (chain.EdgeIds == null) return;

            if (CurrentScenarioChainEdge == null) return;

            var edges = CurrentScenario?.CScenarioPointRegion?.Paths?.Edges;
            if (edges == null) return;


            int lasti = (chain.Edges?.Length ?? 0) - 1;

            var edgeid = 0;
            for (int i = 0; i < chain.Edges.Length; i++)
            {
                if (chain.Edges[i] == CurrentScenarioChainEdge)
                {
                    edgeid = i;
                    break;
                }
            }

            if (!moveDown && (edgeid <= 0)) return;
            if (moveDown && (edgeid >= lasti)) return;

            var swapid = edgeid + (moveDown ? 1 : -1);
            var swaped = chain.Edges[swapid];

            chain.Edges[swapid] = CurrentScenarioChainEdge;
            chain.EdgeIds[swapid] = (ushort)CurrentScenarioChainEdge.EdgeIndex;
            chain.Edges[edgeid] = swaped;
            chain.EdgeIds[edgeid] = (ushort)swapid;

            var ce = CurrentScenarioChainEdge;

            LoadScenarioChainTabPage();

            CurrentScenarioChainEdge = ce;
            ProjectForm.SetScenarioChainEdge(CurrentScenarioChainEdge);

            ScenarioChainEdgesListBox.SelectedItem = ce;

            //LoadScenarioChainEdgeTabPage();

        }

        private void UpdateScenarioChainEdgeLinkage()
        {
            if (CurrentScenarioChainEdge == null) return;
            if (CurrentScenario == null) return;


            var chains = CurrentScenario?.CScenarioPointRegion?.Paths;
            if (chains == null) return;

            var nodes = chains.Nodes;
            if (nodes == null) return;

            ushort nifrom = CurrentScenarioChainEdge.NodeIndexFrom;
            ushort nito = CurrentScenarioChainEdge.NodeIndexTo;

            if (nifrom < nodes.Length) CurrentScenarioChainEdge.NodeFrom = nodes[nifrom];
            if (nito < nodes.Length) CurrentScenarioChainEdge.NodeTo = nodes[nito];

            ////need to rebuild the link verts.. updating the graphics should do it...
            if (ProjectForm.WorldForm != null)
            {
                ProjectForm.WorldForm.UpdateScenarioGraphics(CurrentScenario, false);
            }
        }










        private void ScenarioPointAddToProjectButton_Click(object sender, EventArgs e)
        {
            ProjectForm.AddScenarioToProject(CurrentScenario);
        }

        private void ScenarioPointDeleteButton_Click(object sender, EventArgs e)
        {
            ProjectForm.SetProjectItem(CurrentScenarioNode);
            ProjectForm.DeleteScenarioNode();
        }

        private void ScenarioPointGoToButton_Click(object sender, EventArgs e)
        {
            if (CurrentScenarioNode == null) return;
            if (ProjectForm.WorldForm == null) return;
            ProjectForm.WorldForm.GoToPosition(CurrentScenarioNode.Position);
        }

        private void ScenarioPointPositionTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentScenarioNode == null) return;
            Vector3 v = FloatUtil.ParseVector3String(ScenarioPointPositionTextBox.Text);
            bool change = false;
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CurrentScenarioNode.Position != v)
                {
                    CurrentScenarioNode.SetPosition(v);
                    ProjectForm.SetScenarioHasChanged(true);
                    change = true;
                }
            }
            if (change)
            {
                if (ProjectForm.WorldForm != null)
                {
                    ProjectForm.WorldForm.SetWidgetPosition(CurrentScenarioNode.Position);
                    ProjectForm.WorldForm.UpdateScenarioGraphics(CurrentScenario, false);
                }
            }
        }

        private void ScenarioPointDirectionTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentScenarioNode == null) return;
            if (CurrentScenarioNode.MyPoint == null) return;
            float dir = FloatUtil.Parse(ScenarioPointDirectionTextBox.Text);
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CurrentScenarioNode.MyPoint.Direction != dir)
                {
                    CurrentScenarioNode.MyPoint.Direction = dir;
                    CurrentScenarioNode.Orientation = CurrentScenarioNode.MyPoint.Orientation;
                    ProjectForm.SetScenarioHasChanged(true);
                    if (ProjectForm.WorldForm != null)
                    {
                        ProjectForm.WorldForm.SetWidgetRotation(CurrentScenarioNode.Orientation);
                    }
                }
            }
        }

        private void ScenarioPointTypeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentScenarioNode == null) return;
            if (CurrentScenarioNode.MyPoint == null) return;
            ScenarioType stype = ScenarioPointTypeComboBox.SelectedItem as ScenarioType;
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CurrentScenarioNode.MyPoint.Type != stype)
                {
                    CurrentScenarioNode.MyPoint.Type = stype;
                    ProjectForm.SetScenarioHasChanged(true);
                }
            }
            ProjectForm.ProjectExplorer?.UpdateScenarioNodeTreeNode(CurrentScenarioNode);

            if (CurrentScenarioNode.ChainingNode != null)
            {
                ScenarioChainNodeTypeComboBox.SelectedItem = stype;
            }
        }

        private void ScenarioPointModelSetComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentScenarioNode == null) return;
            if (CurrentScenarioNode.MyPoint == null) return;
            AmbientModelSet mset = ScenarioPointModelSetComboBox.SelectedItem as AmbientModelSet;
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CurrentScenarioNode.MyPoint.ModelSet != mset)
                {
                    CurrentScenarioNode.MyPoint.ModelSet = mset;
                    ProjectForm.SetScenarioHasChanged(true);
                }
            }
            ProjectForm.ProjectExplorer?.UpdateScenarioNodeTreeNode(CurrentScenarioNode);
        }

        private void ScenarioPointInteriorTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentScenarioNode == null) return;
            if (CurrentScenarioNode.MyPoint == null) return;
            uint hash = 0;
            string name = ScenarioPointInteriorTextBox.Text;
            if (!uint.TryParse(name, out hash))//don't re-hash hashes
            {
                hash = JenkHash.GenHash(name);
                JenkIndex.Ensure(name);
            }
            ScenarioPointInteriorHashLabel.Text = "Hash: " + hash.ToString();
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CurrentScenarioNode.MyPoint.InteriorName != hash)
                {
                    CurrentScenarioNode.MyPoint.InteriorName = hash;
                    ProjectForm.SetScenarioHasChanged(true);
                }
            }
        }

        private void ScenarioPointGroupTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentScenarioNode == null) return;
            if (CurrentScenarioNode.MyPoint == null) return;
            uint hash = 0;
            string name = ScenarioPointGroupTextBox.Text;
            if (!uint.TryParse(name, out hash))//don't re-hash hashes
            {
                hash = JenkHash.GenHash(name);
                JenkIndex.Ensure(name);
            }
            ScenarioPointGroupHashLabel.Text = "Hash: " + hash.ToString();
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CurrentScenarioNode.MyPoint.GroupName != hash)
                {
                    CurrentScenarioNode.MyPoint.GroupName = hash;
                    ProjectForm.SetScenarioHasChanged(true);
                }
            }
        }

        private void ScenarioPointImapTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentScenarioNode == null) return;
            if (CurrentScenarioNode.MyPoint == null) return;
            uint hash = 0;
            string name = ScenarioPointImapTextBox.Text;
            if (!uint.TryParse(name, out hash))//don't re-hash hashes
            {
                hash = JenkHash.GenHash(name);
                JenkIndex.Ensure(name);
            }
            ScenarioPointImapHashLabel.Text = "Hash: " + hash.ToString();
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CurrentScenarioNode.MyPoint.IMapName != hash)
                {
                    CurrentScenarioNode.MyPoint.IMapName = hash;
                    ProjectForm.SetScenarioHasChanged(true);
                }
            }
        }

        private void ScenarioPointTimeStartUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentScenarioNode == null) return;
            if (CurrentScenarioNode.MyPoint == null) return;
            byte t = (byte)ScenarioPointTimeStartUpDown.Value;
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CurrentScenarioNode.MyPoint.TimeStart != t)
                {
                    CurrentScenarioNode.MyPoint.TimeStart = t;
                    ProjectForm.SetScenarioHasChanged(true);
                }
            }
            ProjectForm.ProjectExplorer?.UpdateScenarioNodeTreeNode(CurrentScenarioNode);
        }

        private void ScenarioPointTimeEndUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentScenarioNode == null) return;
            if (CurrentScenarioNode.MyPoint == null) return;
            byte t = (byte)ScenarioPointTimeEndUpDown.Value;
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CurrentScenarioNode.MyPoint.TimeEnd != t)
                {
                    CurrentScenarioNode.MyPoint.TimeEnd = t;
                    ProjectForm.SetScenarioHasChanged(true);
                }
            }
            ProjectForm.ProjectExplorer?.UpdateScenarioNodeTreeNode(CurrentScenarioNode);
        }

        private void ScenarioPointProbabilityUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentScenarioNode == null) return;
            if (CurrentScenarioNode.MyPoint == null) return;
            byte v = (byte)ScenarioPointProbabilityUpDown.Value;
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CurrentScenarioNode.MyPoint.Probability != v)
                {
                    CurrentScenarioNode.MyPoint.Probability = v;
                    ProjectForm.SetScenarioHasChanged(true);
                }
            }
        }

        private void ScenarioPointSpOnlyFlagUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentScenarioNode == null) return;
            if (CurrentScenarioNode.MyPoint == null) return;
            byte v = (byte)ScenarioPointSpOnlyFlagUpDown.Value;
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CurrentScenarioNode.MyPoint.AvailableMpSp != v)
                {
                    CurrentScenarioNode.MyPoint.AvailableMpSp = v;
                    ProjectForm.SetScenarioHasChanged(true);
                }
            }
        }

        private void ScenarioPointRadiusUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentScenarioNode == null) return;
            if (CurrentScenarioNode.MyPoint == null) return;
            byte v = (byte)ScenarioPointRadiusUpDown.Value;
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CurrentScenarioNode.MyPoint.Radius != v)
                {
                    CurrentScenarioNode.MyPoint.Radius = v;
                    ProjectForm.SetScenarioHasChanged(true);
                }
            }
        }

        private void ScenarioPointWaitTimeUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentScenarioNode == null) return;
            if (CurrentScenarioNode.MyPoint == null) return;
            byte v = (byte)ScenarioPointWaitTimeUpDown.Value;
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CurrentScenarioNode.MyPoint.WaitTime != v)
                {
                    CurrentScenarioNode.MyPoint.WaitTime = v;
                    ProjectForm.SetScenarioHasChanged(true);
                }
            }
        }

        private void ScenarioPointFlagsValueUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentScenarioNode == null) return;
            if (CurrentScenarioNode.MyPoint == null) return;
            var iflags = (uint)ScenarioPointFlagsValueUpDown.Value;
            populatingui = true;
            for (int i = 0; i < ScenarioPointFlagsCheckedListBox.Items.Count; i++)
            {
                var c = ((iflags & (1 << i)) > 0);
                ScenarioPointFlagsCheckedListBox.SetItemCheckState(i, c ? CheckState.Checked : CheckState.Unchecked);
            }
            populatingui = false;
            Unk_700327466 f = (Unk_700327466)iflags;
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CurrentScenarioNode.MyPoint.Flags != f)
                {
                    CurrentScenarioNode.MyPoint.Flags = f;
                    ProjectForm.SetScenarioHasChanged(true);
                }
            }
        }

        private void ScenarioPointFlagsCheckedListBox_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (populatingui) return;
            if (CurrentScenarioNode == null) return;
            if (CurrentScenarioNode.MyPoint == null) return;
            uint iflags = 0;
            for (int i = 0; i < ScenarioPointFlagsCheckedListBox.Items.Count; i++)
            {
                if (e.Index == i)
                {
                    if (e.NewValue == CheckState.Checked)
                    {
                        iflags += (uint)(1 << i);
                    }
                }
                else
                {
                    if (ScenarioPointFlagsCheckedListBox.GetItemChecked(i))
                    {
                        iflags += (uint)(1 << i);
                    }
                }
            }
            populatingui = true;
            ScenarioPointFlagsValueUpDown.Value = iflags;
            populatingui = false;
            Unk_700327466 f = (Unk_700327466)iflags;
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CurrentScenarioNode.MyPoint.Flags != f)
                {
                    CurrentScenarioNode.MyPoint.Flags = f;
                    ProjectForm.SetScenarioHasChanged(true);
                }
            }
        }



        private void ScenarioEntityAddToProjectButton_Click(object sender, EventArgs e)
        {
            ProjectForm.AddScenarioToProject(CurrentScenario);
        }

        private void ScenarioEntityDeleteButton_Click(object sender, EventArgs e)
        {
            ProjectForm.DeleteScenarioEntity();
        }

        private void ScenarioEntityGoToButton_Click(object sender, EventArgs e)
        {
            if (CurrentScenarioNode == null) return;
            if (ProjectForm.WorldForm == null) return;
            ProjectForm.WorldForm.GoToPosition(CurrentScenarioNode.Position);
        }

        private void ScenarioEntityPositionTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentScenarioNode == null) return;
            if (CurrentScenarioNode.Entity == null) return;
            Vector3 v = FloatUtil.ParseVector3String(ScenarioEntityPositionTextBox.Text);
            bool change = false;
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CurrentScenarioNode.Position != v)
                {
                    CurrentScenarioNode.SetPosition(v);
                    ProjectForm.SetScenarioHasChanged(true);
                    change = true;
                }
            }
            if (change)
            {
                if (ProjectForm.WorldForm != null)
                {
                    ProjectForm.WorldForm.SetWidgetPosition(CurrentScenarioNode.Position);
                    ProjectForm.WorldForm.UpdateScenarioGraphics(CurrentScenario, false);
                }
            }
        }

        private void ScenarioEntityTypeTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentScenarioNode == null) return;
            if (CurrentScenarioNode.Entity == null) return;
            uint hash = 0;
            string name = ScenarioEntityTypeTextBox.Text;
            if (!uint.TryParse(name, out hash))//don't re-hash hashes
            {
                hash = JenkHash.GenHash(name);
                JenkIndex.Ensure(name);
            }
            ScenarioEntityTypeHashLabel.Text = "Hash: " + hash.ToString();
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CurrentScenarioNode.Entity.TypeName != hash)
                {
                    CurrentScenarioNode.Entity.TypeName = hash;
                    ProjectForm.SetScenarioHasChanged(true);
                }
            }
            ProjectForm.ProjectExplorer?.UpdateScenarioNodeTreeNode(CurrentScenarioNode);
        }

        private void ScenarioEntityUnk1UpDown_ValueChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentScenarioNode == null) return;
            if (CurrentScenarioNode.Entity == null) return;
            byte v = (byte)ScenarioEntityUnk1UpDown.Value;
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CurrentScenarioNode.Entity.Unk1 != v)
                {
                    CurrentScenarioNode.Entity.Unk1 = v;
                    ProjectForm.SetScenarioHasChanged(true);
                }
            }
        }

        private void ScenarioEntityUnk2UpDown_ValueChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentScenarioNode == null) return;
            if (CurrentScenarioNode.Entity == null) return;
            byte v = (byte)ScenarioEntityUnk2UpDown.Value;
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CurrentScenarioNode.Entity.Unk2 != v)
                {
                    CurrentScenarioNode.Entity.Unk2 = v;
                    ProjectForm.SetScenarioHasChanged(true);
                }
            }
        }

        private void ScenarioEntityPointsListBox_DoubleClick(object sender, EventArgs e)
        {
            var item = ScenarioEntityPointsListBox.SelectedItem as MCExtensionDefSpawnPoint;
            if (item == null) return;

            var nodes = CurrentScenario?.ScenarioRegion?.Nodes;
            if (nodes == null) return;

            ScenarioNode node = null;
            foreach (var snode in nodes)
            {
                if (snode.EntityPoint == item)
                {
                    node = snode;
                    break;
                }
            }

            if (node == null) return;

            ProjectForm.ProjectExplorer?.TrySelectScenarioNodeTreeNode(node);

        }

        private void ScenarioEntityAddPointButton_Click(object sender, EventArgs e)
        {
            ProjectForm.AddScenarioEntityPoint();
        }



        private void ScenarioEntityPointAddToProjectButton_Click(object sender, EventArgs e)
        {
            ProjectForm.AddScenarioToProject(CurrentScenario);
        }

        private void ScenarioEntityPointDeleteButton_Click(object sender, EventArgs e)
        {
            ProjectForm.SetProjectItem(CurrentScenarioNode);
            ProjectForm.DeleteScenarioNode();
        }

        private void ScenarioEntityPointGoToButton_Click(object sender, EventArgs e)
        {
            if (CurrentScenarioNode == null) return;
            if (ProjectForm.WorldForm == null) return;
            ProjectForm.WorldForm.GoToPosition(CurrentScenarioNode.Position);
        }

        private void ScenarioEntityPointPositionTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentScenarioNode == null) return;
            if (CurrentScenarioNode.EntityPoint == null) return;
            Vector3 v = FloatUtil.ParseVector3String(ScenarioEntityPointPositionTextBox.Text);
            v += CurrentScenarioNode.EntityPoint.ParentPosition;
            bool change = false;
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CurrentScenarioNode.Position != v)
                {
                    CurrentScenarioNode.SetPosition(v);
                    ProjectForm.SetScenarioHasChanged(true);
                    change = true;
                }
            }
            if (change)
            {
                if (ProjectForm.WorldForm != null)
                {
                    ProjectForm.WorldForm.SetWidgetPosition(CurrentScenarioNode.Position);
                    ProjectForm.WorldForm.UpdateScenarioGraphics(CurrentScenario, false);
                }
            }
        }

        private void ScenarioEntityPointRotationTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentScenarioNode == null) return;
            if (CurrentScenarioNode.EntityPoint == null) return;
            Vector4 v = FloatUtil.ParseVector4String(ScenarioEntityPointRotationTextBox.Text);
            bool change = false;
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CurrentScenarioNode.EntityPoint.OffsetRotation != v)
                {
                    CurrentScenarioNode.EntityPoint.OffsetRotation = v;
                    CurrentScenarioNode.Orientation = new Quaternion(v);
                    ProjectForm.SetScenarioHasChanged(true);
                    change = true;
                }
            }
            if (change)
            {
                if (ProjectForm.WorldForm != null)
                {
                    ProjectForm.WorldForm.SetWidgetRotation(CurrentScenarioNode.Orientation);
                }
            }
        }

        private void ScenarioEntityPointNameTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentScenarioNode == null) return;
            if (CurrentScenarioNode.EntityPoint == null) return;
            uint hash = 0;
            string name = ScenarioEntityPointNameTextBox.Text;
            if (!uint.TryParse(name, out hash))//don't re-hash hashes
            {
                hash = JenkHash.GenHash(name);
                JenkIndex.Ensure(name);
            }
            ScenarioEntityPointNameHashLabel.Text = "Hash: " + hash.ToString();
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CurrentScenarioNode.EntityPoint.NameHash != hash)
                {
                    CurrentScenarioNode.EntityPoint.NameHash = hash;
                    ProjectForm.SetScenarioHasChanged(true);
                }
            }
            ProjectForm.ProjectExplorer?.UpdateScenarioNodeTreeNode(CurrentScenarioNode);
        }

        private void ScenarioEntityPointSpawnTypeTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentScenarioNode == null) return;
            if (CurrentScenarioNode.EntityPoint == null) return;
            uint hash = 0;
            string name = ScenarioEntityPointSpawnTypeTextBox.Text;
            if (!uint.TryParse(name, out hash))//don't re-hash hashes
            {
                hash = JenkHash.GenHash(name);
                JenkIndex.Ensure(name);
            }
            ScenarioEntityPointSpawnTypeHashLabel.Text = "Hash: " + hash.ToString();
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CurrentScenarioNode.EntityPoint.SpawnType != hash)
                {
                    CurrentScenarioNode.EntityPoint.SpawnType = hash;
                    ProjectForm.SetScenarioHasChanged(true);
                }
            }
            ProjectForm.ProjectExplorer?.UpdateScenarioNodeTreeNode(CurrentScenarioNode);
        }

        private void ScenarioEntityPointPedTypeTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentScenarioNode == null) return;
            if (CurrentScenarioNode.EntityPoint == null) return;
            uint hash = 0;
            string name = ScenarioEntityPointPedTypeTextBox.Text;
            if (!uint.TryParse(name, out hash))//don't re-hash hashes
            {
                hash = JenkHash.GenHash(name);
                JenkIndex.Ensure(name);
            }
            ScenarioEntityPointPedTypeHashLabel.Text = "Hash: " + hash.ToString();
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CurrentScenarioNode.EntityPoint.PedType != hash)
                {
                    CurrentScenarioNode.EntityPoint.PedType = hash;
                    ProjectForm.SetScenarioHasChanged(true);
                }
            }
            ProjectForm.ProjectExplorer?.UpdateScenarioNodeTreeNode(CurrentScenarioNode);
        }

        private void ScenarioEntityPointGroupTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentScenarioNode == null) return;
            if (CurrentScenarioNode.EntityPoint == null) return;
            uint hash = 0;
            string name = ScenarioEntityPointGroupTextBox.Text;
            if (!uint.TryParse(name, out hash))//don't re-hash hashes
            {
                hash = JenkHash.GenHash(name);
                JenkIndex.Ensure(name);
            }
            ScenarioEntityPointGroupHashLabel.Text = "Hash: " + hash.ToString();
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CurrentScenarioNode.EntityPoint.Group != hash)
                {
                    CurrentScenarioNode.EntityPoint.Group = hash;
                    ProjectForm.SetScenarioHasChanged(true);
                }
            }
        }

        private void ScenarioEntityPointInteriorTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentScenarioNode == null) return;
            if (CurrentScenarioNode.EntityPoint == null) return;
            uint hash = 0;
            string name = ScenarioEntityPointInteriorTextBox.Text;
            if (!uint.TryParse(name, out hash))//don't re-hash hashes
            {
                hash = JenkHash.GenHash(name);
                JenkIndex.Ensure(name);
            }
            ScenarioEntityPointInteriorHashLabel.Text = "Hash: " + hash.ToString();
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CurrentScenarioNode.EntityPoint.Interior != hash)
                {
                    CurrentScenarioNode.EntityPoint.Interior = hash;
                    ProjectForm.SetScenarioHasChanged(true);
                }
            }
        }

        private void ScenarioEntityPointRequiredImapTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentScenarioNode == null) return;
            if (CurrentScenarioNode.EntityPoint == null) return;
            uint hash = 0;
            string name = ScenarioEntityPointRequiredImapTextBox.Text;
            if (!uint.TryParse(name, out hash))//don't re-hash hashes
            {
                hash = JenkHash.GenHash(name);
                JenkIndex.Ensure(name);
            }
            ScenarioEntityPointRequiredImapHashLabel.Text = "Hash: " + hash.ToString();
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CurrentScenarioNode.EntityPoint.RequiredImap != hash)
                {
                    CurrentScenarioNode.EntityPoint.RequiredImap = hash;
                    ProjectForm.SetScenarioHasChanged(true);
                }
            }
        }

        private void ScenarioEntityPointAvailableInMpSpComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentScenarioNode == null) return;
            if (CurrentScenarioNode.EntityPoint == null) return;
            Unk_3573596290 v = (Unk_3573596290)ScenarioEntityPointAvailableInMpSpComboBox.SelectedItem;
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CurrentScenarioNode.EntityPoint.AvailableInMpSp != v)
                {
                    CurrentScenarioNode.EntityPoint.AvailableInMpSp = v;
                    ProjectForm.SetScenarioHasChanged(true);
                }
            }
        }

        private void ScenarioEntityPointProbabilityTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentScenarioNode == null) return;
            if (CurrentScenarioNode.EntityPoint == null) return;
            float v = FloatUtil.Parse(ScenarioEntityPointProbabilityTextBox.Text);
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CurrentScenarioNode.EntityPoint.Probability != v)
                {
                    CurrentScenarioNode.EntityPoint.Probability = v;
                    ProjectForm.SetScenarioHasChanged(true);
                }
            }
        }

        private void ScenarioEntityPointTimeTillPedLeavesTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentScenarioNode == null) return;
            if (CurrentScenarioNode.EntityPoint == null) return;
            float v = FloatUtil.Parse(ScenarioEntityPointTimeTillPedLeavesTextBox.Text);
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CurrentScenarioNode.EntityPoint.TimeTillPedLeaves != v)
                {
                    CurrentScenarioNode.EntityPoint.TimeTillPedLeaves = v;
                    ProjectForm.SetScenarioHasChanged(true);
                }
            }
        }

        private void ScenarioEntityPointRadiusTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentScenarioNode == null) return;
            if (CurrentScenarioNode.EntityPoint == null) return;
            float v = FloatUtil.Parse(ScenarioEntityPointRadiusTextBox.Text);
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CurrentScenarioNode.EntityPoint.Radius != v)
                {
                    CurrentScenarioNode.EntityPoint.Radius = v;
                    ProjectForm.SetScenarioHasChanged(true);
                }
            }
        }

        private void ScenarioEntityPointStartUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentScenarioNode == null) return;
            if (CurrentScenarioNode.EntityPoint == null) return;
            byte v = (byte)ScenarioEntityPointStartUpDown.Value;
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CurrentScenarioNode.EntityPoint.StartTime != v)
                {
                    CurrentScenarioNode.EntityPoint.StartTime = v;
                    ProjectForm.SetScenarioHasChanged(true);
                }
            }
        }

        private void ScenarioEntityPointEndUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentScenarioNode == null) return;
            if (CurrentScenarioNode.EntityPoint == null) return;
            byte v = (byte)ScenarioEntityPointEndUpDown.Value;
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CurrentScenarioNode.EntityPoint.EndTime != v)
                {
                    CurrentScenarioNode.EntityPoint.EndTime = v;
                    ProjectForm.SetScenarioHasChanged(true);
                }
            }
        }

        private void ScenarioEntityPointExtendedRangeCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentScenarioNode == null) return;
            if (CurrentScenarioNode.EntityPoint == null) return;
            bool v = ScenarioEntityPointExtendedRangeCheckBox.Checked;
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CurrentScenarioNode.EntityPoint.ExtendedRange != v)
                {
                    CurrentScenarioNode.EntityPoint.ExtendedRange = v;
                    ProjectForm.SetScenarioHasChanged(true);
                }
            }
        }

        private void ScenarioEntityPointShortRangeCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentScenarioNode == null) return;
            if (CurrentScenarioNode.EntityPoint == null) return;
            bool v = ScenarioEntityPointShortRangeCheckBox.Checked;
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CurrentScenarioNode.EntityPoint.ShortRange != v)
                {
                    CurrentScenarioNode.EntityPoint.ShortRange = v;
                    ProjectForm.SetScenarioHasChanged(true);
                }
            }
        }

        private void ScenarioEntityPointHighPriCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentScenarioNode == null) return;
            if (CurrentScenarioNode.EntityPoint == null) return;
            bool v = ScenarioEntityPointHighPriCheckBox.Checked;
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CurrentScenarioNode.EntityPoint.HighPri != v)
                {
                    CurrentScenarioNode.EntityPoint.HighPri = v;
                    ProjectForm.SetScenarioHasChanged(true);
                }
            }
        }

        private void ScenarioEntityPointFlagsUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentScenarioNode == null) return;
            if (CurrentScenarioNode.EntityPoint == null) return;
            var iflags = (uint)ScenarioEntityPointFlagsUpDown.Value;
            populatingui = true;
            for (int i = 0; i < ScenarioEntityPointFlagsCheckedListBox.Items.Count; i++)
            {
                var c = ((iflags & (1 << i)) > 0);
                ScenarioEntityPointFlagsCheckedListBox.SetItemCheckState(i, c ? CheckState.Checked : CheckState.Unchecked);
            }
            populatingui = false;
            Unk_700327466 f = (Unk_700327466)iflags;
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CurrentScenarioNode.EntityPoint.Flags != f)
                {
                    CurrentScenarioNode.EntityPoint.Flags = f;
                    ProjectForm.SetScenarioHasChanged(true);
                }
            }
        }

        private void ScenarioEntityPointFlagsCheckedListBox_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (populatingui) return;
            if (CurrentScenarioNode == null) return;
            if (CurrentScenarioNode.EntityPoint == null) return;
            uint iflags = 0;
            for (int i = 0; i < ScenarioEntityPointFlagsCheckedListBox.Items.Count; i++)
            {
                if (e.Index == i)
                {
                    if (e.NewValue == CheckState.Checked)
                    {
                        iflags += (uint)(1 << i);
                    }
                }
                else
                {
                    if (ScenarioEntityPointFlagsCheckedListBox.GetItemChecked(i))
                    {
                        iflags += (uint)(1 << i);
                    }
                }
            }
            populatingui = true;
            ScenarioEntityPointFlagsUpDown.Value = iflags;
            populatingui = false;
            Unk_700327466 f = (Unk_700327466)iflags;
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CurrentScenarioNode.EntityPoint.Flags != f)
                {
                    CurrentScenarioNode.EntityPoint.Flags = f;
                    ProjectForm.SetScenarioHasChanged(true);
                }
            }
        }



        private void ScenarioChainNodeAddToProjectButton_Click(object sender, EventArgs e)
        {
            ProjectForm.AddScenarioToProject(CurrentScenario);
        }

        private void ScenarioChainNodeDeleteButton_Click(object sender, EventArgs e)
        {
            ProjectForm.SetProjectItem(CurrentScenarioNode);
            ProjectForm.DeleteScenarioNode();
        }

        private void ScenarioChainNodeGoToButton_Click(object sender, EventArgs e)
        {
            if (CurrentScenarioNode == null) return;
            if (ProjectForm.WorldForm == null) return;
            ProjectForm.WorldForm.GoToPosition(CurrentScenarioNode.Position);
        }

        private void ScenarioChainNodePositionTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentScenarioNode == null) return;
            Vector3 v = FloatUtil.ParseVector3String(ScenarioChainNodePositionTextBox.Text);
            bool change = false;
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CurrentScenarioNode.Position != v)
                {
                    CurrentScenarioNode.SetPosition(v);
                    ProjectForm.SetScenarioHasChanged(true);
                    change = true;
                }
            }
            if (change)
            {
                if (ProjectForm.WorldForm != null)
                {
                    ProjectForm.WorldForm.SetWidgetPosition(CurrentScenarioNode.Position);
                    ProjectForm.WorldForm.UpdateScenarioGraphics(CurrentScenario, false);
                }
            }
        }

        private void ScenarioChainNodeUnk1TextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentScenarioNode == null) return;
            if (CurrentScenarioNode.ChainingNode == null) return;
            uint hash = 0;
            string name = ScenarioChainNodeUnk1TextBox.Text;
            if (!uint.TryParse(name, out hash))//don't re-hash hashes
            {
                hash = JenkHash.GenHash(name);
                JenkIndex.Ensure(name);
            }
            ScenarioChainNodeUnk1HashLabel.Text = "Hash: " + hash.ToString();
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CurrentScenarioNode.ChainingNode.Unk1 != hash)
                {
                    CurrentScenarioNode.ChainingNode.Unk1 = hash;
                    ProjectForm.SetScenarioHasChanged(true);
                }
            }
        }

        private void ScenarioChainNodeTypeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentScenarioNode == null) return;
            if (CurrentScenarioNode.ChainingNode == null) return;
            ScenarioType stype = ScenarioChainNodeTypeComboBox.SelectedItem as ScenarioType;
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CurrentScenarioNode.ChainingNode.Type != stype)
                {
                    CurrentScenarioNode.ChainingNode.Type = stype;
                    CurrentScenarioNode.ChainingNode.TypeHash = stype?.NameHash ?? 0;
                    ProjectForm.SetScenarioHasChanged(true);
                }
            }
            ProjectForm.ProjectExplorer?.UpdateScenarioNodeTreeNode(CurrentScenarioNode);
        }

        private void ScenarioChainNodeFirstCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentScenarioNode == null) return;
            if (CurrentScenarioNode.ChainingNode == null) return;
            bool v = !ScenarioChainNodeFirstCheckBox.Checked;
            bool change = false;
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CurrentScenarioNode.ChainingNode.NotFirst != v)
                {
                    CurrentScenarioNode.ChainingNode.NotFirst = v;
                    ProjectForm.SetScenarioHasChanged(true);
                    change = true;
                }
            }
            if (change)
            {
                if (ProjectForm.WorldForm != null)
                {
                    ProjectForm.WorldForm.UpdateScenarioGraphics(CurrentScenario, false);
                }
            }
        }

        private void ScenarioChainNodeLastCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentScenarioNode == null) return;
            if (CurrentScenarioNode.ChainingNode == null) return;
            bool v = !ScenarioChainNodeLastCheckBox.Checked;
            bool change = false;
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CurrentScenarioNode.ChainingNode.NotLast != v)
                {
                    CurrentScenarioNode.ChainingNode.NotLast = v;
                    ProjectForm.SetScenarioHasChanged(true);
                    change = true;
                }
            }
            if (change)
            {
                if (ProjectForm.WorldForm != null)
                {
                    ProjectForm.WorldForm.UpdateScenarioGraphics(CurrentScenario, false);
                }
            }
        }


        private void ScenarioChainAddToProjectButton_Click(object sender, EventArgs e)
        {
            ProjectForm.AddScenarioToProject(CurrentScenario);
        }

        private void ScenarioChainDeleteButton_Click(object sender, EventArgs e)
        {
            ProjectForm.DeleteScenarioChain();
        }

        private void ScenarioChainEdgesListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            CurrentScenarioChainEdge = ScenarioChainEdgesListBox.SelectedItem as MCScenarioChainingEdge;
            ProjectForm.SetScenarioChainEdge(CurrentScenarioChainEdge);
            populatingui = true;
            LoadScenarioChainEdgeTabPage();
            populatingui = false;
        }

        private void ScenarioChainAddEdgeButton_Click(object sender, EventArgs e)
        {
            AddScenarioChainEdge();
        }

        private void ScenarioChainRemoveEdgeButton_Click(object sender, EventArgs e)
        {
            RemoveScenarioChainEdge();
        }

        private void ScenarioChainMoveEdgeUpButton_Click(object sender, EventArgs e)
        {
            MoveScenarioChainEdge(false);
        }

        private void ScenarioChainMoveEdgeDownButton_Click(object sender, EventArgs e)
        {
            MoveScenarioChainEdge(true);
        }

        private void ScenarioChainUnk1UpDown_ValueChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentScenarioNode == null) return;
            if (CurrentScenarioNode.ChainingNode == null) return;
            if (CurrentScenarioNode.ChainingNode.Chain == null) return;
            byte v = (byte)ScenarioChainUnk1UpDown.Value;
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CurrentScenarioNode.ChainingNode.Chain.Unk1 != v)
                {
                    CurrentScenarioNode.ChainingNode.Chain.Unk1 = v;
                    ProjectForm.SetScenarioHasChanged(true);
                }
            }
        }


        private void ScenarioChainEdgeNodeIndexFromUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentScenarioChainEdge == null) return;
            ushort nodeid = (ushort)ScenarioChainEdgeNodeIndexFromUpDown.Value;
            bool change = false;
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CurrentScenarioChainEdge.NodeIndexFrom != nodeid)
                {
                    CurrentScenarioChainEdge.NodeIndexFrom = nodeid;
                    ProjectForm.SetScenarioHasChanged(true);
                    change = true;
                }
            }
            if (change)
            {
                UpdateScenarioChainEdgeLinkage();

                //refresh the selected item in the list box, to update the text, and the other controls.
                ScenarioChainEdgesListBox.Items[ScenarioChainEdgesListBox.SelectedIndex] = ScenarioChainEdgesListBox.SelectedItem;
            }
        }

        private void ScenarioChainEdgeNodeIndexToUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentScenarioChainEdge == null) return;
            ushort nodeid = (ushort)ScenarioChainEdgeNodeIndexToUpDown.Value;
            bool change = false;
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CurrentScenarioChainEdge.NodeIndexTo != nodeid)
                {
                    CurrentScenarioChainEdge.NodeIndexTo = nodeid;
                    ProjectForm.SetScenarioHasChanged(true);
                    change = true;
                }
            }
            if (change)
            {
                UpdateScenarioChainEdgeLinkage();

                //refresh the selected item in the list box, to update the text, and the other controls.
                ScenarioChainEdgesListBox.Items[ScenarioChainEdgesListBox.SelectedIndex] = ScenarioChainEdgesListBox.SelectedItem;
            }
        }

        private void ScenarioChainEdgeActionComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentScenarioChainEdge == null) return;
            Unk_3609807418 v = (Unk_3609807418)ScenarioChainEdgeActionComboBox.SelectedItem;
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CurrentScenarioChainEdge.Action != v)
                {
                    CurrentScenarioChainEdge.Action = v;
                    ProjectForm.SetScenarioHasChanged(true);
                }
            }
        }

        private void ScenarioChainEdgeNavModeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentScenarioChainEdge == null) return;
            Unk_3971773454 v = (Unk_3971773454)ScenarioChainEdgeNavModeComboBox.SelectedItem;
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CurrentScenarioChainEdge.NavMode != v)
                {
                    CurrentScenarioChainEdge.NavMode = v;
                    ProjectForm.SetScenarioHasChanged(true);
                }
            }
        }

        private void ScenarioChainEdgeNavSpeedComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentScenarioChainEdge == null) return;
            Unk_941086046 v = (Unk_941086046)ScenarioChainEdgeNavSpeedComboBox.SelectedItem;
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CurrentScenarioChainEdge.NavSpeed != v)
                {
                    CurrentScenarioChainEdge.NavSpeed = v;
                    ProjectForm.SetScenarioHasChanged(true);
                }
            }
        }



        private void ScenarioClusterAddToProjectButton_Click(object sender, EventArgs e)
        {
            ProjectForm.AddScenarioToProject(CurrentScenario);
        }

        private void ScenarioClusterDeleteButton_Click(object sender, EventArgs e)
        {
            ProjectForm.DeleteScenarioCluster();
        }

        private void ScenarioClusterGoToButton_Click(object sender, EventArgs e)
        {
            if (CurrentScenarioNode == null) return;
            if (ProjectForm.WorldForm == null) return;
            ProjectForm.WorldForm.GoToPosition(CurrentScenarioNode.Position);
        }

        private void ScenarioClusterCenterTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentScenarioNode == null) return;
            Vector3 v = FloatUtil.ParseVector3String(ScenarioClusterCenterTextBox.Text);
            bool change = false;
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CurrentScenarioNode.Position != v)
                {
                    CurrentScenarioNode.SetPosition(v);
                    ProjectForm.SetScenarioHasChanged(true);
                    change = true;
                }
                if ((CurrentScenarioNode.Cluster != null) && (CurrentScenarioNode.Cluster.Position != v))
                {
                    CurrentScenarioNode.Cluster.Position = v;
                    ProjectForm.SetScenarioHasChanged(true);
                    change = true;
                }
            }
            if (change)
            {
                if (ProjectForm.WorldForm != null)
                {
                    ProjectForm.WorldForm.SetWidgetPosition(CurrentScenarioNode.Position);
                    ProjectForm.WorldForm.UpdateScenarioGraphics(CurrentScenario, false);
                }
            }
        }

        private void ScenarioClusterRadiusTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentScenarioNode == null) return;
            float r = FloatUtil.Parse(ScenarioClusterRadiusTextBox.Text);
            bool change = false;
            lock (ProjectForm.ProjectSyncRoot)
            {
                if ((CurrentScenarioNode.Cluster != null) && (CurrentScenarioNode.Cluster.Radius != r))
                {
                    CurrentScenarioNode.Cluster.Radius = r;
                    ProjectForm.SetScenarioHasChanged(true);
                    change = true;
                }
            }
            if (change)
            {
                if (ProjectForm.WorldForm != null)
                {
                    ProjectForm.WorldForm.SetWidgetPosition(CurrentScenarioNode.Position);
                    ProjectForm.WorldForm.UpdateScenarioGraphics(CurrentScenario, false);
                }
            }
        }

        private void ScenarioClusterUnk1TextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentScenarioNode == null) return;
            if (CurrentScenarioNode.Cluster == null) return;
            float v = FloatUtil.Parse(ScenarioClusterUnk1TextBox.Text);
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CurrentScenarioNode.Cluster.Unk1 != v)
                {
                    CurrentScenarioNode.Cluster.Unk1 = v;
                    ProjectForm.SetScenarioHasChanged(true);
                }
            }
        }

        private void ScenarioClusterUnk2CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentScenarioNode == null) return;
            if (CurrentScenarioNode.Cluster == null) return;
            bool v = ScenarioClusterUnk2CheckBox.Checked;
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CurrentScenarioNode.Cluster.Unk2 != v)
                {
                    CurrentScenarioNode.Cluster.Unk2 = v;
                    ProjectForm.SetScenarioHasChanged(true);
                }
            }
        }

        private void ScenarioClusterPointsListBox_DoubleClick(object sender, EventArgs e)
        {
            var item = ScenarioClusterPointsListBox.SelectedItem as MCScenarioPoint;
            if (item == null) return;

            var nodes = CurrentScenario?.ScenarioRegion?.Nodes;
            if (nodes == null) return;

            ScenarioNode node = null;
            foreach (var snode in nodes)
            {
                if (snode.ClusterMyPoint == item)
                {
                    node = snode;
                    break;
                }
            }

            if (node == null) return;

            ProjectForm.ProjectExplorer?.TrySelectScenarioNodeTreeNode(node);

        }

        private void ScenarioClusterAddPointButton_Click(object sender, EventArgs e)
        {
            ProjectForm.AddScenarioClusterPoint();
        }



        private void ScenarioClusterPointAddToProjectButton_Click(object sender, EventArgs e)
        {
            ProjectForm.AddScenarioToProject(CurrentScenario);
        }

        private void ScenarioClusterPointDeleteButton_Click(object sender, EventArgs e)
        {
            ProjectForm.SetProjectItem(CurrentScenarioNode);
            ProjectForm.DeleteScenarioNode();
        }

        private void ScenarioClusterPointGoToButton_Click(object sender, EventArgs e)
        {
            if (CurrentScenarioNode == null) return;
            if (ProjectForm.WorldForm == null) return;
            ProjectForm.WorldForm.GoToPosition(CurrentScenarioNode.Position);
        }

        private void ScenarioClusterPointPositionTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentScenarioNode == null) return;
            Vector3 v = FloatUtil.ParseVector3String(ScenarioClusterPointPositionTextBox.Text);
            bool change = false;
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CurrentScenarioNode.Position != v)
                {
                    CurrentScenarioNode.SetPosition(v);
                    ProjectForm.SetScenarioHasChanged(true);
                    change = true;
                }
            }
            if (change)
            {
                if (ProjectForm.WorldForm != null)
                {
                    ProjectForm.WorldForm.SetWidgetPosition(CurrentScenarioNode.Position);
                    ProjectForm.WorldForm.UpdateScenarioGraphics(CurrentScenario, false);
                }
            }
        }

        private void ScenarioClusterPointDirectionTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentScenarioNode == null) return;
            if (CurrentScenarioNode.ClusterMyPoint == null) return;
            float dir = FloatUtil.Parse(ScenarioClusterPointDirectionTextBox.Text);
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CurrentScenarioNode.ClusterMyPoint.Direction != dir)
                {
                    CurrentScenarioNode.ClusterMyPoint.Direction = dir;
                    CurrentScenarioNode.Orientation = CurrentScenarioNode.ClusterMyPoint.Orientation;
                    ProjectForm.SetScenarioHasChanged(true);
                    if (ProjectForm.WorldForm != null)
                    {
                        ProjectForm.WorldForm.SetWidgetRotation(CurrentScenarioNode.Orientation);
                    }
                }
            }
        }

        private void ScenarioClusterPointTypeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentScenarioNode == null) return;
            if (CurrentScenarioNode.ClusterMyPoint == null) return;
            ScenarioType stype = ScenarioClusterPointTypeComboBox.SelectedItem as ScenarioType;
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CurrentScenarioNode.ClusterMyPoint.Type != stype)
                {
                    CurrentScenarioNode.ClusterMyPoint.Type = stype;
                    ProjectForm.SetScenarioHasChanged(true);
                }
            }
            ProjectForm.ProjectExplorer?.UpdateScenarioNodeTreeNode(CurrentScenarioNode);

            if (CurrentScenarioNode.ChainingNode != null)
            {
                ScenarioChainNodeTypeComboBox.SelectedItem = stype;
            }
        }

        private void ScenarioClusterPointModelSetComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentScenarioNode == null) return;
            if (CurrentScenarioNode.ClusterMyPoint == null) return;
            AmbientModelSet mset = ScenarioClusterPointModelSetComboBox.SelectedItem as AmbientModelSet;
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CurrentScenarioNode.ClusterMyPoint.ModelSet != mset)
                {
                    CurrentScenarioNode.ClusterMyPoint.ModelSet = mset;
                    ProjectForm.SetScenarioHasChanged(true);
                }
            }
            ProjectForm.ProjectExplorer?.UpdateScenarioNodeTreeNode(CurrentScenarioNode);
        }

        private void ScenarioClusterPointInteriorTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentScenarioNode == null) return;
            if (CurrentScenarioNode.ClusterMyPoint == null) return;
            uint hash = 0;
            string name = ScenarioClusterPointInteriorTextBox.Text;
            if (!uint.TryParse(name, out hash))//don't re-hash hashes
            {
                hash = JenkHash.GenHash(name);
                JenkIndex.Ensure(name);
            }
            ScenarioClusterPointInteriorHashLabel.Text = "Hash: " + hash.ToString();
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CurrentScenarioNode.ClusterMyPoint.InteriorName != hash)
                {
                    CurrentScenarioNode.ClusterMyPoint.InteriorName = hash;
                    ProjectForm.SetScenarioHasChanged(true);
                }
            }
        }

        private void ScenarioClusterPointGroupTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentScenarioNode == null) return;
            if (CurrentScenarioNode.ClusterMyPoint == null) return;
            uint hash = 0;
            string name = ScenarioClusterPointGroupTextBox.Text;
            if (!uint.TryParse(name, out hash))//don't re-hash hashes
            {
                hash = JenkHash.GenHash(name);
                JenkIndex.Ensure(name);
            }
            ScenarioClusterPointGroupHashLabel.Text = "Hash: " + hash.ToString();
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CurrentScenarioNode.ClusterMyPoint.GroupName != hash)
                {
                    CurrentScenarioNode.ClusterMyPoint.GroupName = hash;
                    ProjectForm.SetScenarioHasChanged(true);
                }
            }
        }

        private void ScenarioClusterPointImapTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentScenarioNode == null) return;
            if (CurrentScenarioNode.ClusterMyPoint == null) return;
            uint hash = 0;
            string name = ScenarioClusterPointImapTextBox.Text;
            if (!uint.TryParse(name, out hash))//don't re-hash hashes
            {
                hash = JenkHash.GenHash(name);
                JenkIndex.Ensure(name);
            }
            ScenarioClusterPointImapHashLabel.Text = "Hash: " + hash.ToString();
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CurrentScenarioNode.ClusterMyPoint.IMapName != hash)
                {
                    CurrentScenarioNode.ClusterMyPoint.IMapName = hash;
                    ProjectForm.SetScenarioHasChanged(true);
                }
            }
        }

        private void ScenarioClusterPointTimeStartUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentScenarioNode == null) return;
            if (CurrentScenarioNode.ClusterMyPoint == null) return;
            byte t = (byte)ScenarioClusterPointTimeStartUpDown.Value;
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CurrentScenarioNode.ClusterMyPoint.TimeStart != t)
                {
                    CurrentScenarioNode.ClusterMyPoint.TimeStart = t;
                    ProjectForm.SetScenarioHasChanged(true);
                }
            }
            ProjectForm.ProjectExplorer?.UpdateScenarioNodeTreeNode(CurrentScenarioNode);
        }

        private void ScenarioClusterPointTimeEndUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentScenarioNode == null) return;
            if (CurrentScenarioNode.ClusterMyPoint == null) return;
            byte t = (byte)ScenarioClusterPointTimeEndUpDown.Value;
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CurrentScenarioNode.ClusterMyPoint.TimeEnd != t)
                {
                    CurrentScenarioNode.ClusterMyPoint.TimeEnd = t;
                    ProjectForm.SetScenarioHasChanged(true);
                }
            }
            ProjectForm.ProjectExplorer?.UpdateScenarioNodeTreeNode(CurrentScenarioNode);
        }

        private void ScenarioClusterPointProbabilityUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentScenarioNode == null) return;
            if (CurrentScenarioNode.ClusterMyPoint == null) return;
            byte v = (byte)ScenarioClusterPointProbabilityUpDown.Value;
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CurrentScenarioNode.ClusterMyPoint.Probability != v)
                {
                    CurrentScenarioNode.ClusterMyPoint.Probability = v;
                    ProjectForm.SetScenarioHasChanged(true);
                }
            }
        }

        private void ScenarioClusterPointSpOnlyFlagUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentScenarioNode == null) return;
            if (CurrentScenarioNode.ClusterMyPoint == null) return;
            byte v = (byte)ScenarioClusterPointSpOnlyFlagUpDown.Value;
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CurrentScenarioNode.ClusterMyPoint.AvailableMpSp != v)
                {
                    CurrentScenarioNode.ClusterMyPoint.AvailableMpSp = v;
                    ProjectForm.SetScenarioHasChanged(true);
                }
            }
        }

        private void ScenarioClusterPointRadiusUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentScenarioNode == null) return;
            if (CurrentScenarioNode.ClusterMyPoint == null) return;
            byte v = (byte)ScenarioClusterPointRadiusUpDown.Value;
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CurrentScenarioNode.ClusterMyPoint.Radius != v)
                {
                    CurrentScenarioNode.ClusterMyPoint.Radius = v;
                    ProjectForm.SetScenarioHasChanged(true);
                }
            }
        }

        private void ScenarioClusterPointWaitTimeUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentScenarioNode == null) return;
            if (CurrentScenarioNode.ClusterMyPoint == null) return;
            byte v = (byte)ScenarioClusterPointWaitTimeUpDown.Value;
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CurrentScenarioNode.ClusterMyPoint.WaitTime != v)
                {
                    CurrentScenarioNode.ClusterMyPoint.WaitTime = v;
                    ProjectForm.SetScenarioHasChanged(true);
                }
            }
        }

        private void ScenarioClusterPointFlagsUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentScenarioNode == null) return;
            if (CurrentScenarioNode.ClusterMyPoint == null) return;
            var iflags = (uint)ScenarioClusterPointFlagsUpDown.Value;
            populatingui = true;
            for (int i = 0; i < ScenarioClusterPointFlagsCheckedListBox.Items.Count; i++)
            {
                var c = ((iflags & (1 << i)) > 0);
                ScenarioClusterPointFlagsCheckedListBox.SetItemCheckState(i, c ? CheckState.Checked : CheckState.Unchecked);
            }
            populatingui = false;
            Unk_700327466 f = (Unk_700327466)iflags;
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CurrentScenarioNode.ClusterMyPoint.Flags != f)
                {
                    CurrentScenarioNode.ClusterMyPoint.Flags = f;
                    ProjectForm.SetScenarioHasChanged(true);
                }
            }
        }

        private void ScenarioClusterPointFlagsCheckedListBox_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (populatingui) return;
            if (CurrentScenarioNode == null) return;
            if (CurrentScenarioNode.ClusterMyPoint == null) return;
            uint iflags = 0;
            for (int i = 0; i < ScenarioClusterPointFlagsCheckedListBox.Items.Count; i++)
            {
                if (e.Index == i)
                {
                    if (e.NewValue == CheckState.Checked)
                    {
                        iflags += (uint)(1 << i);
                    }
                }
                else
                {
                    if (ScenarioClusterPointFlagsCheckedListBox.GetItemChecked(i))
                    {
                        iflags += (uint)(1 << i);
                    }
                }
            }
            populatingui = true;
            ScenarioClusterPointFlagsUpDown.Value = iflags;
            populatingui = false;
            Unk_700327466 f = (Unk_700327466)iflags;
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CurrentScenarioNode.ClusterMyPoint.Flags != f)
                {
                    CurrentScenarioNode.ClusterMyPoint.Flags = f;
                    ProjectForm.SetScenarioHasChanged(true);
                }
            }
        }



    }
}
