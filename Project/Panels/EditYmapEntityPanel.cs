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
    public partial class EditYmapEntityPanel : ProjectPanel
    {
        public ProjectForm ProjectForm;
        public YmapEntityDef CurrentEntity { get; set; }

        private bool populatingui = false;

        public EditYmapEntityPanel(ProjectForm owner)
        {
            ProjectForm = owner;
            InitializeComponent();
            LoadDropDowns();
        }

        public void SetEntity(YmapEntityDef entity)
        {
            CurrentEntity = entity;
            Tag = entity;
            LoadEntity();
            UpdateFormTitle();
        }

        private void UpdateFormTitle()
        {
            Text = CurrentEntity?.Name ?? "Entity";
        }


        private void LoadDropDowns()
        {
            EntityLodLevelComboBox.Items.Clear();
            EntityLodLevelComboBox.Items.Add(Unk_1264241711.LODTYPES_DEPTH_ORPHANHD);
            EntityLodLevelComboBox.Items.Add(Unk_1264241711.LODTYPES_DEPTH_HD);
            EntityLodLevelComboBox.Items.Add(Unk_1264241711.LODTYPES_DEPTH_LOD);
            EntityLodLevelComboBox.Items.Add(Unk_1264241711.LODTYPES_DEPTH_SLOD1);
            EntityLodLevelComboBox.Items.Add(Unk_1264241711.LODTYPES_DEPTH_SLOD2);
            EntityLodLevelComboBox.Items.Add(Unk_1264241711.LODTYPES_DEPTH_SLOD3);
            EntityLodLevelComboBox.Items.Add(Unk_1264241711.LODTYPES_DEPTH_SLOD4);

            EntityPriorityLevelComboBox.Items.Clear();
            EntityPriorityLevelComboBox.Items.Add(Unk_648413703.PRI_REQUIRED);
            EntityPriorityLevelComboBox.Items.Add(Unk_648413703.PRI_OPTIONAL_HIGH);
            EntityPriorityLevelComboBox.Items.Add(Unk_648413703.PRI_OPTIONAL_MEDIUM);
            EntityPriorityLevelComboBox.Items.Add(Unk_648413703.PRI_OPTIONAL_LOW);
        }


        private void LoadEntity()
        {
            if (CurrentEntity == null)
            {
                //EntityPanel.Enabled = false;
                EntityAddToProjectButton.Enabled = false;
                EntityDeleteButton.Enabled = false;
                EntityArchetypeTextBox.Text = string.Empty;
                EntityArchetypeHashLabel.Text = "Hash: 0";
                EntityFlagsTextBox.Text = string.Empty;
                EntityGuidTextBox.Text = string.Empty;
                EntityPositionTextBox.Text = string.Empty;
                EntityRotationTextBox.Text = string.Empty;
                EntityScaleXYTextBox.Text = string.Empty;
                EntityScaleZTextBox.Text = string.Empty;
                EntityParentIndexTextBox.Text = string.Empty;
                EntityLodDistTextBox.Text = string.Empty;
                EntityChildLodDistTextBox.Text = string.Empty;
                EntityLodLevelComboBox.SelectedIndex = 0;// Math.Max(EntityLodLevelComboBox.FindString(), 0);
                EntityNumChildrenTextBox.Text = string.Empty;
                EntityPriorityLevelComboBox.SelectedIndex = 0; //Math.Max(..
                EntityAOMultiplierTextBox.Text = string.Empty;
                EntityArtificialAOTextBox.Text = string.Empty;
                EntityTintValueTextBox.Text = string.Empty;
                EntityPivotEditCheckBox.Checked = false;
                EntityPivotPositionTextBox.Text = string.Empty;
                EntityPivotRotationTextBox.Text = string.Empty;
                foreach (int i in EntityFlagsCheckedListBox.CheckedIndices)
                {
                    EntityFlagsCheckedListBox.SetItemCheckState(i, CheckState.Unchecked);
                }
            }
            else
            {
                populatingui = true;
                var e = CurrentEntity.CEntityDef;
                var po = CurrentEntity.PivotOrientation;
                //EntityPanel.Enabled = true;
                EntityAddToProjectButton.Enabled = !ProjectForm.YmapExistsInProject(CurrentEntity.Ymap);
                EntityDeleteButton.Enabled = !EntityAddToProjectButton.Enabled;
                EntityArchetypeTextBox.Text = e.archetypeName.ToString();
                EntityArchetypeHashLabel.Text = "Hash: " + e.archetypeName.Hash.ToString();
                EntityFlagsTextBox.Text = e.flags.ToString();
                EntityGuidTextBox.Text = e.guid.ToString();
                EntityPositionTextBox.Text = FloatUtil.GetVector3String(e.position);
                EntityRotationTextBox.Text = FloatUtil.GetVector4String(e.rotation);
                EntityScaleXYTextBox.Text = FloatUtil.ToString(e.scaleXY);
                EntityScaleZTextBox.Text = FloatUtil.ToString(e.scaleZ);
                EntityParentIndexTextBox.Text = e.parentIndex.ToString();
                EntityLodDistTextBox.Text = FloatUtil.ToString(e.lodDist);
                EntityChildLodDistTextBox.Text = FloatUtil.ToString(e.childLodDist);
                EntityLodLevelComboBox.SelectedIndex = Math.Max(EntityLodLevelComboBox.FindString(e.lodLevel.ToString()), 0);
                EntityNumChildrenTextBox.Text = e.numChildren.ToString();
                EntityPriorityLevelComboBox.SelectedIndex = Math.Max(EntityPriorityLevelComboBox.FindString(e.priorityLevel.ToString()), 0);
                EntityAOMultiplierTextBox.Text = e.ambientOcclusionMultiplier.ToString();
                EntityArtificialAOTextBox.Text = e.artificialAmbientOcclusion.ToString();
                EntityTintValueTextBox.Text = e.tintValue.ToString();
                EntityPivotPositionTextBox.Text = FloatUtil.GetVector3String(CurrentEntity.PivotPosition);
                EntityPivotRotationTextBox.Text = FloatUtil.GetVector4String(new Vector4(po.X, po.Y, po.Z, po.W));
                for (int i = 0; i < EntityFlagsCheckedListBox.Items.Count; i++)
                {
                    var cv = ((e.flags & (1u << i)) > 0);
                    EntityFlagsCheckedListBox.SetItemCheckState(i, cv ? CheckState.Checked : CheckState.Unchecked);
                }
                populatingui = false;



                ProjectForm.WorldForm?.SelectEntity(CurrentEntity); //hopefully the drawable is already loaded - this will try get from cache

                ////struct CEntityDef:
                //MetaHash archetypeName { get; set; } //8   8: Hash: 0: archetypeName
                //uint flags { get; set; } //12   12: UnsignedInt: 0: flags
                //uint guid { get; set; } //16   16: UnsignedInt: 0: guid
                //Vector3 position { get; set; } //32   32: Float_XYZ: 0: position
                //Vector4 rotation { get; set; } //48   48: Float_XYZW: 0: rotation
                //float scaleXY { get; set; } //64   64: Float: 0: 2627937847
                //float scaleZ { get; set; } //68   68: Float: 0: 284916802
                //int parentIndex { get; set; } //72   72: SignedInt: 0: parentIndex
                //float lodDist { get; set; } //76   76: Float: 0: lodDist
                //float childLodDist { get; set; } //80   80: Float: 0: childLodDist//3398912973
                //Unk_1264241711 lodLevel { get; set; } //84   84: IntEnum: 1264241711: lodLevel  //LODTYPES_DEPTH_
                //uint numChildren { get; set; } //88   88: UnsignedInt: 0: numChildren//2793909385
                //Unk_648413703 priorityLevel { get; set; } //92   92: IntEnum: 648413703: priorityLevel//647098393
                //Array_StructurePointer extensions { get; set; } //96   96: Array: 0: extensions  {0: StructurePointer: 0: 256}
                //int ambientOcclusionMultiplier { get; set; } //112   112: SignedInt: 0: ambientOcclusionMultiplier//415356295
                //int artificialAmbientOcclusion { get; set; } //116   116: SignedInt: 0: artificialAmbientOcclusion//599844163
                //uint tintValue { get; set; } //120   120: UnsignedInt: 0: tintValue//1015358759
            }

        }


        private void EntityArchetypeTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentEntity == null) return;
            uint hash = 0;
            string name = EntityArchetypeTextBox.Text;
            if (!uint.TryParse(name, out hash))//don't re-hash hashes
            {
                hash = JenkHash.GenHash(name);
                JenkIndex.Ensure(name);
            }
            EntityArchetypeHashLabel.Text = "Hash: " + hash.ToString();

            var arch = ProjectForm.GameFileCache.GetArchetype(hash);
            if (arch == null)
            {
                EntityArchetypeHashLabel.Text += " (not found)";
            }

            TreeNode tn = ProjectForm.ProjectExplorer?.FindEntityTreeNode(CurrentEntity);
            if (tn != null)
            {
                tn.Text = name;
            }

            if (CurrentEntity != null)
            {
                lock (ProjectForm.ProjectSyncRoot)
                {
                    CurrentEntity._CEntityDef.archetypeName = new MetaHash(hash);
                    if (CurrentEntity.Archetype != arch)
                    {
                        CurrentEntity.SetArchetype(arch);
                        ProjectForm.SetYmapHasChanged(true);
                    }
                }
            }
        }

        private void EntityFlagsTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentEntity == null) return;
            uint flags = 0;
            uint.TryParse(EntityFlagsTextBox.Text, out flags);
            populatingui = true;
            for (int i = 0; i < EntityFlagsCheckedListBox.Items.Count; i++)
            {
                var c = ((flags & (1u << i)) > 0);
                EntityFlagsCheckedListBox.SetItemCheckState(i, c ? CheckState.Checked : CheckState.Unchecked);
            }
            populatingui = false;
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CurrentEntity._CEntityDef.flags != flags)
                {
                    CurrentEntity._CEntityDef.flags = flags;
                    ProjectForm.SetYmapHasChanged(true);
                }
            }
        }

        private void EntityFlagsCheckedListBox_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (populatingui) return;
            if (CurrentEntity == null) return;
            uint flags = 0;
            for (int i = 0; i < EntityFlagsCheckedListBox.Items.Count; i++)
            {
                if (e.Index == i)
                {
                    if (e.NewValue == CheckState.Checked)
                    {
                        flags += (uint)(1 << i);
                    }
                }
                else
                {
                    if (EntityFlagsCheckedListBox.GetItemChecked(i))
                    {
                        flags += (uint)(1 << i);
                    }
                }
            }
            populatingui = true;
            EntityFlagsTextBox.Text = flags.ToString();
            populatingui = false;
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CurrentEntity._CEntityDef.flags != flags)
                {
                    CurrentEntity._CEntityDef.flags = flags;
                    ProjectForm.SetYmapHasChanged(true);
                }
            }
        }

        private void EntityGuidTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentEntity == null) return;
            uint guid = 0;
            uint.TryParse(EntityGuidTextBox.Text, out guid);
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CurrentEntity._CEntityDef.guid != guid)
                {
                    CurrentEntity._CEntityDef.guid = guid;
                    ProjectForm.SetYmapHasChanged(true);
                }
            }
        }

        private void EntityPositionTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentEntity == null) return;
            Vector3 v = FloatUtil.ParseVector3String(EntityPositionTextBox.Text);
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CurrentEntity.MloParent != null)
                {
                    //TODO: positioning for interior entities!
                }
                else
                {
                    if (CurrentEntity.Position != v)
                    {
                        CurrentEntity.SetPosition(v);
                        ProjectForm.SetYmapHasChanged(true);
                        var wf = ProjectForm.WorldForm;
                        if (wf != null)
                        {
                            wf.BeginInvoke(new Action(() =>
                            {
                                wf.SetWidgetPosition(CurrentEntity.WidgetPosition, true);
                            }));
                        }
                    }
                }
            }
        }

        private void EntityRotationTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentEntity == null) return;
            Vector4 v = FloatUtil.ParseVector4String(EntityRotationTextBox.Text);
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CurrentEntity._CEntityDef.rotation != v)
                {
                    Quaternion q = new Quaternion(v);
                    CurrentEntity.SetOrientationInv(q);
                    ProjectForm.SetYmapHasChanged(true);
                    var wf = ProjectForm.WorldForm;
                    if (wf != null)
                    {
                        wf.BeginInvoke(new Action(() =>
                        {
                            wf.SetWidgetRotation(CurrentEntity.WidgetOrientation, true);
                        }));
                    }
                }
            }
        }

        private void EntityScaleXYTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentEntity == null) return;
            float sxy = 0;
            FloatUtil.TryParse(EntityScaleXYTextBox.Text, out sxy);
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CurrentEntity.Scale.X != sxy)
                {
                    Vector3 newscale = new Vector3(sxy, sxy, CurrentEntity.Scale.Z);
                    CurrentEntity.SetScale(newscale);
                    ProjectForm.SetYmapHasChanged(true);
                    var wf = ProjectForm.WorldForm;
                    if (wf != null)
                    {
                        wf.BeginInvoke(new Action(() =>
                        {
                            wf.SetWidgetScale(newscale, true);
                        }));
                    }
                }
            }
        }

        private void EntityScaleZTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentEntity == null) return;
            float sz = 0;
            FloatUtil.TryParse(EntityScaleZTextBox.Text, out sz);
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CurrentEntity.Scale.Z != sz)
                {
                    Vector3 newscale = new Vector3(CurrentEntity.Scale.X, CurrentEntity.Scale.Y, sz);
                    CurrentEntity.SetScale(newscale);
                    ProjectForm.SetYmapHasChanged(true);
                    var wf = ProjectForm.WorldForm;
                    if (wf != null)
                    {
                        wf.BeginInvoke(new Action(() =>
                        {
                            wf.SetWidgetScale(newscale, true);
                        }));
                    }
                }
            }
        }

        private void EntityParentIndexTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentEntity == null) return;
            int pind = 0;
            int.TryParse(EntityParentIndexTextBox.Text, out pind);
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CurrentEntity._CEntityDef.parentIndex != pind)
                {
                    CurrentEntity._CEntityDef.parentIndex = pind; //Needs more work for LOD linking!
                    ProjectForm.SetYmapHasChanged(true);
                }
            }
        }

        private void EntityLodDistTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentEntity == null) return;
            float lodDist = 0;
            FloatUtil.TryParse(EntityLodDistTextBox.Text, out lodDist);
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CurrentEntity._CEntityDef.lodDist != lodDist)
                {
                    CurrentEntity._CEntityDef.lodDist = lodDist;
                    ProjectForm.SetYmapHasChanged(true);
                }
            }
        }

        private void EntityChildLodDistTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentEntity == null) return;
            float childLodDist = 0;
            FloatUtil.TryParse(EntityChildLodDistTextBox.Text, out childLodDist);
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CurrentEntity._CEntityDef.childLodDist != childLodDist)
                {
                    CurrentEntity._CEntityDef.childLodDist = childLodDist;
                    ProjectForm.SetYmapHasChanged(true);
                }
            }
        }

        private void EntityLodLevelComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentEntity == null) return;
            Unk_1264241711 lodLevel = (Unk_1264241711)EntityLodLevelComboBox.SelectedItem;
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CurrentEntity._CEntityDef.lodLevel != lodLevel)
                {
                    CurrentEntity._CEntityDef.lodLevel = lodLevel;
                    ProjectForm.SetYmapHasChanged(true);
                }
            }
        }

        private void EntityNumChildrenTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentEntity == null) return;
            uint numChildren = 0;
            uint.TryParse(EntityNumChildrenTextBox.Text, out numChildren);
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CurrentEntity._CEntityDef.numChildren != numChildren)
                {
                    CurrentEntity._CEntityDef.numChildren = numChildren;
                    ProjectForm.SetYmapHasChanged(true);
                }
            }
        }

        private void EntityPriorityLevelComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentEntity == null) return;
            Unk_648413703 priorityLevel = (Unk_648413703)EntityPriorityLevelComboBox.SelectedItem;
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CurrentEntity._CEntityDef.priorityLevel != priorityLevel)
                {
                    CurrentEntity._CEntityDef.priorityLevel = priorityLevel;
                    ProjectForm.SetYmapHasChanged(true);
                }
            }
        }

        private void EntityAOMultiplierTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentEntity == null) return;
            int aomult = 0;
            int.TryParse(EntityAOMultiplierTextBox.Text, out aomult);
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CurrentEntity._CEntityDef.ambientOcclusionMultiplier != aomult)
                {
                    CurrentEntity._CEntityDef.ambientOcclusionMultiplier = aomult;
                    ProjectForm.SetYmapHasChanged(true);
                }
            }
        }

        private void EntityArtificialAOTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentEntity == null) return;
            int artao = 0;
            int.TryParse(EntityArtificialAOTextBox.Text, out artao);
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CurrentEntity._CEntityDef.artificialAmbientOcclusion != artao)
                {
                    CurrentEntity._CEntityDef.artificialAmbientOcclusion = artao;
                    ProjectForm.SetYmapHasChanged(true);
                }
            }
        }

        private void EntityTintValueTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentEntity == null) return;
            uint tintValue = 0;
            uint.TryParse(EntityTintValueTextBox.Text, out tintValue);
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CurrentEntity._CEntityDef.tintValue != tintValue)
                {
                    CurrentEntity._CEntityDef.tintValue = tintValue;
                    ProjectForm.SetYmapHasChanged(true);
                }
            }
        }

        private void EntityGoToButton_Click(object sender, EventArgs e)
        {
            if (CurrentEntity == null) return;
            if (ProjectForm.WorldForm == null) return;
            ProjectForm.WorldForm.GoToPosition(CurrentEntity.Position);
        }

        private void EntityNormalizeRotationButton_Click(object sender, EventArgs e)
        {
            Vector4 v = FloatUtil.ParseVector4String(EntityRotationTextBox.Text);
            Quaternion q = Quaternion.Normalize(new Quaternion(v));
            EntityRotationTextBox.Text = FloatUtil.GetVector4String(new Vector4(q.X, q.Y, q.Z, q.W));
        }

        private void EntityAddToProjectButton_Click(object sender, EventArgs e)
        {
            ProjectForm.SetProjectItem(CurrentEntity);
            ProjectForm.AddEntityToProject();
        }

        private void EntityDeleteButton_Click(object sender, EventArgs e)
        {
            ProjectForm.SetProjectItem(CurrentEntity);
            ProjectForm.DeleteEntity();
        }

        private void EntityPivotEditCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (ProjectForm.WorldForm != null)
            {
                ProjectForm.WorldForm.EditEntityPivot = EntityPivotEditCheckBox.Checked;
            }
        }

        private void EntityPivotPositionTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentEntity == null) return;
            Vector3 v = FloatUtil.ParseVector3String(EntityPivotPositionTextBox.Text);
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CurrentEntity.PivotPosition != v)
                {
                    CurrentEntity.SetPivotPosition(v);
                    //SetYmapHasChanged(true);
                    var wf = ProjectForm.WorldForm;
                    if (wf != null)
                    {
                        wf.BeginInvoke(new Action(() =>
                        {
                            bool editpivot = wf.EditEntityPivot;
                            wf.EditEntityPivot = true;
                            wf.SetWidgetPosition(CurrentEntity.WidgetPosition, true);
                            wf.EditEntityPivot = editpivot;
                        }));
                    }
                }
            }
        }

        private void EntityPivotRotationTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentEntity == null) return;
            Vector4 v = FloatUtil.ParseVector4String(EntityPivotRotationTextBox.Text);
            Quaternion q = new Quaternion(v);
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CurrentEntity.PivotOrientation != q)
                {
                    CurrentEntity.SetPivotOrientation(q);
                    //SetYmapHasChanged(true);
                    var wf = ProjectForm.WorldForm;
                    if (wf != null)
                    {
                        wf.BeginInvoke(new Action(() =>
                        {
                            bool editpivot = wf.EditEntityPivot;
                            wf.EditEntityPivot = true;
                            wf.SetWidgetRotation(CurrentEntity.WidgetOrientation, true);
                            wf.EditEntityPivot = editpivot;
                        }));
                    }
                }
            }
        }

        private void EntityPivotRotationNormalizeButton_Click(object sender, EventArgs e)
        {
            Vector4 v = FloatUtil.ParseVector4String(EntityPivotRotationTextBox.Text);
            Quaternion q = Quaternion.Normalize(new Quaternion(v));
            EntityPivotRotationTextBox.Text = FloatUtil.GetVector4String(new Vector4(q.X, q.Y, q.Z, q.W));
        }
    }
}
