using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CodeWalker.GameFiles;
using CodeWalker.World;
using SharpDX;

// TODO
// - COMPLETED -- Optimization feature.
// - COMPLETED -- Remove grass instances using CTRL + SHIFT + LMB

// - Better gizmo for grass brush (like a circle with a little line in the middle sticking upwards)
// - Maybe some kind of auto coloring system? I've noticed that mostly all grass in GTA inherits it's color from the surface it's on.
// - Grass area fill (generate grass on ydr based on colision materials?)
// - Need to have a way to erase instances from other batches in the current batches ymap.
//   if we optimize our instances, we'd have to go through each batch to erase, this is very monotonous.

// BUG
// - I've added a "zoom" kind of feature when hitting the goto button, but when the bounds of the 
// grass batch are 0, the zoom of the camera is set to 0, which causes the end-user to have to scroll
// out a lot in order to use any movement controls. I will need to clamp that to a minimum value.

namespace CodeWalker.Project.Panels
{
    public partial class EditYmapGrassPanel : ProjectPanel
    {
        public ProjectForm ProjectForm;

        public EditYmapGrassPanel(ProjectForm owner)
        {
            ProjectForm = owner;
            InitializeComponent();
        }

        public YmapGrassInstanceBatch CurrentBatch { get; set; }

        #region Form

        public void SetBatch(YmapGrassInstanceBatch batch)
        {
            CurrentBatch = batch;
            Tag = batch;
            UpdateFormTitle();
            UpdateControls();
            ProjectForm.WorldForm?.SelectObject(batch);
        }

        private void UpdateControls()
        {
            if (ProjectForm?.CurrentProjectFile == null) return;
            if (ProjectForm.GrassBatchExistsInProject(CurrentBatch))
            {
                GrassAddToProjectButton.Enabled = false;
                GrassDeleteButton.Enabled = true;
            }
            else
            {
                GrassAddToProjectButton.Enabled = true;
                GrassDeleteButton.Enabled = false;
            }

            ArchetypeNameTextBox.Text = CurrentBatch.Batch.archetypeName.ToString();
            PositionTextBox.Text = FloatUtil.GetVector3String(CurrentBatch.Position);
            LodDistNumericUpDown.Value = CurrentBatch.Batch.lodDist;
            LodFadeRangeNumericUpDown.Value = (decimal) CurrentBatch.Batch.LodInstFadeRange;
            LodFadeStartDistanceNumericUpDown.Value = (decimal) CurrentBatch.Batch.LodFadeStartDist;
            ScaleRangeTextBox.Text = FloatUtil.GetVector3String(CurrentBatch.Batch.ScaleRange);
            OrientToTerrainNumericUpDown.Value = (decimal)CurrentBatch.Batch.OrientToTerrain;
            OptmizationThresholdNumericUpDown.Value = 15;
            BrushModeCheckBox.Checked = CurrentBatch.BrushEnabled;
            RadiusNumericUpDown.Value = (decimal)CurrentBatch.BrushRadius;
            ExtentsMinTextBox.Text = FloatUtil.GetVector3String(CurrentBatch.AABBMin);
            ExtentsMaxTextBox.Text = FloatUtil.GetVector3String(CurrentBatch.AABBMax);
        }

        private void UpdateFormTitle()
        {
            Text = CurrentBatch?.Batch.archetypeName.ToString() ?? "Grass Batch";
        }

        #endregion

        #region Events

        #region BrushSettings

        private void GrassGoToButton_Click(object sender, EventArgs e)
        {
            if (CurrentBatch == null) return;
            ProjectForm.WorldForm?.GoToPosition(CurrentBatch.Position, CurrentBatch.AABBMax - CurrentBatch.AABBMin);
        }

        private void GrassAddToProjectButton_Click(object sender, EventArgs e)
        {
            ProjectForm.SetProjectItem(CurrentBatch);
            ProjectForm.AddGrassBatchToProject();
        }

        private void GrassDeleteButton_Click(object sender, EventArgs e)
        {
            ProjectForm.SetProjectItem(CurrentBatch);
            var ymap = CurrentBatch?.Ymap;
            if (!ProjectForm.DeleteGrassBatch()) return;

            ymap?.CalcExtents(); // Recalculate the extents after deleting the grass batch.
            ProjectForm.WorldForm.SelectItem();
        }

        private void GrassColorLabel_Click(object sender, EventArgs e)
        {
            var colDiag = new ColorDialog {Color = GrassColorLabel.BackColor};
            if (colDiag.ShowDialog(this) == DialogResult.OK)
                GrassColorLabel.BackColor = colDiag.Color;
        }

        private void BrushModeCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (CurrentBatch == null) return;
            CurrentBatch.BrushEnabled = BrushModeCheckBox.Checked;
        }

        private void RadiusNumericUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (CurrentBatch == null) return;
            CurrentBatch.BrushRadius = (float)RadiusNumericUpDown.Value;
        }
        #endregion

        #region Batch Settings

        private void ArchetypeNameTextBox_TextChanged(object sender, EventArgs e)
        {
            var archetypeHash = JenkHash.GenHash(ArchetypeNameTextBox.Text);
            var archetype = ProjectForm.GameFileCache.GetArchetype(archetypeHash);
            if (archetype == null)
            {
                HashLabel.Text = $@"Hash: {archetypeHash} (invalid)";
                return;
            }
            CurrentBatch.Archetype = archetype;
            var b = CurrentBatch.Batch;
            b.archetypeName = archetypeHash;
            CurrentBatch.Batch = b;
            ProjectForm.WorldForm.UpdateGrassBatchGraphics(CurrentBatch);
            HashLabel.Text = $@"Hash: {archetypeHash}";
            UpdateFormTitle();
            CurrentBatch.HasChanged = true;
            ProjectForm.SetGrassBatchHasChanged(false);
            ProjectForm.SetYmapHasChanged(true);
        }

        private void LodDistNumericUpDown_ValueChanged(object sender, EventArgs e)
        {
            var batch = CurrentBatch.Batch;
            batch.lodDist = (uint) LodDistNumericUpDown.Value;
            CurrentBatch.Batch = batch;
            ProjectForm.WorldForm.UpdateGrassBatchGraphics(CurrentBatch);
            ProjectForm.SetYmapHasChanged(true);
        }

        private void LodFadeStartDistanceNumericUpDown_ValueChanged(object sender, EventArgs e)
        {
            var batch = CurrentBatch.Batch;
            batch.LodFadeStartDist = (float) LodFadeStartDistanceNumericUpDown.Value;
            CurrentBatch.Batch = batch;
            ProjectForm.WorldForm.UpdateGrassBatchGraphics(CurrentBatch);
            ProjectForm.SetYmapHasChanged(true);
        }

        private void LodFadeRangeNumericUpDown_ValueChanged(object sender, EventArgs e)
        {
            var batch = CurrentBatch.Batch;
            batch.LodInstFadeRange = (float) LodFadeRangeNumericUpDown.Value;
            CurrentBatch.Batch = batch;
            ProjectForm.WorldForm.UpdateGrassBatchGraphics(CurrentBatch);
            ProjectForm.SetYmapHasChanged(true);
        }

        private void OrientToTerrainNumericUpDown_ValueChanged(object sender, EventArgs e)
        {
            var batch = CurrentBatch.Batch;
            batch.OrientToTerrain = (float) OrientToTerrainNumericUpDown.Value;
            CurrentBatch.Batch = batch;
            ProjectForm.WorldForm.UpdateGrassBatchGraphics(CurrentBatch);
            ProjectForm.SetYmapHasChanged(true);
        }

        private void ScaleRangeTextBox_TextChanged(object sender, EventArgs e)
        {
            var batch = CurrentBatch.Batch;
            var v = FloatUtil.ParseVector3String(ScaleRangeTextBox.Text);
            batch.ScaleRange = v;
            CurrentBatch.Batch = batch;
            ProjectForm.WorldForm.UpdateGrassBatchGraphics(CurrentBatch);
            ProjectForm.SetYmapHasChanged(true);
        }

        private void OptimizeBatchButton_Click(object sender, EventArgs e)
        {
            if (CurrentBatch.Instances == null || CurrentBatch.Instances.Length <= 0) return;
            var d = MessageBox.Show(
                @"You are about to split the selected batch into multiple parts. Are you sure you want to do this?",
                @"Instance Optimizer", MessageBoxButtons.YesNo);

            if (d == DialogResult.No)
                return;

            lock (ProjectForm.WorldForm.RenderSyncRoot)
            {
                var newBatches = CurrentBatch?.OptimizeInstances(CurrentBatch, (float)OptmizationThresholdNumericUpDown.Value);
                if (newBatches == null || newBatches.Length <= 0) return;

                // Remove our batch from the ymap
                CurrentBatch.Ymap.RemoveGrassBatch(CurrentBatch);
                foreach (var batch in newBatches)
                {
                    var b = batch.Batch;
                    b.lodDist = CurrentBatch.Batch.lodDist;
                    b.LodInstFadeRange = CurrentBatch.Batch.LodInstFadeRange;
                    b.LodFadeStartDist = CurrentBatch.Batch.LodFadeStartDist;
                    b.ScaleRange = CurrentBatch.Batch.ScaleRange;
                    b.OrientToTerrain = CurrentBatch.Batch.OrientToTerrain;
                    b.archetypeName = CurrentBatch.Batch.archetypeName;
                    batch.Batch = b;
                    batch.Archetype = CurrentBatch.Archetype;
                    batch.UpdateInstanceCount();
                    ProjectForm.NewGrassBatch(batch);
                }
                CurrentBatch.Ymap.CalcExtents();
                CurrentBatch.Ymap.Save();

                // TODO: Select the last grass batch in the new list on the project explorer.
                ProjectForm.ProjectExplorer.TrySelectGrassBatchTreeNode(CurrentBatch.Ymap.GrassInstanceBatches[0]);
            }
        }

        #endregion

        #endregion

        #region Publics

        public void CreateInstancesAtMouse(SpaceRayIntersectResult mouseRay)
        {
            var wf = ProjectForm.WorldForm;
            if (wf == null) return;

            lock (wf.RenderSyncRoot)
            {
                CurrentBatch.CreateInstancesAtMouse(
                    CurrentBatch,
                    mouseRay,
                    (float) RadiusNumericUpDown.Value,
                    (int) DensityNumericUpDown.Value,
                    SpawnRayFunc,
                    new Color(GrassColorLabel.BackColor.R, GrassColorLabel.BackColor.G, GrassColorLabel.BackColor.B),
                    (int) AoNumericUpDown.Value,
                    (int) ScaleNumericUpDown.Value,
                    FloatUtil.ParseVector3String(PadTextBox.Text),
                    RandomizeScaleCheckBox.Checked
                );
                wf.UpdateGrassBatchGraphics(CurrentBatch);
            }

            BatchChanged();
        }

        public void EraseInstancesAtMouse(SpaceRayIntersectResult mouseRay)
        {
            var wf = ProjectForm.WorldForm;
            if (wf == null) return;
            var changed = false;
            lock (wf.RenderSyncRoot)
            {
                if (CurrentBatch.EraseInstancesAtMouse(
                    CurrentBatch,
                    mouseRay,
                    (float) RadiusNumericUpDown.Value
                ))
                {
                    wf.UpdateGrassBatchGraphics(CurrentBatch);
                    changed = true;
                }
            }

            if (changed) BatchChanged();
        }

        #endregion

        #region Privates

        private SpaceRayIntersectResult SpawnRayFunc(Vector3 spawnPos)
        {
            var res = ProjectForm.WorldForm.Raycast(new Ray(spawnPos, -Vector3.UnitZ));
            return res;
        }

        private void BatchChanged()
        {
            UpdateControls();
            CurrentBatch.UpdateInstanceCount();
            CurrentBatch.HasChanged = true;
            ProjectForm.SetGrassBatchHasChanged(false);
        }

        #endregion
    }
}