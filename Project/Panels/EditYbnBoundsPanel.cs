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
    public partial class EditYbnBoundsPanel : ProjectPanel
    {
        public ProjectForm ProjectForm;
        public Bounds CollisionBounds { get; set; }
        public BoundGeometry CollisionGeom { get; set; }

        private bool populatingui = false;
        private bool waschanged = false;

        public EditYbnBoundsPanel(ProjectForm projectForm)
        {
            ProjectForm = projectForm;
            InitializeComponent();

            LoadDropDowns();
        }

        private void LoadDropDowns()
        {
            MaterialCombo.Items.Clear();
            if (BoundsMaterialTypes.Materials == null) return;
            foreach (var mat in BoundsMaterialTypes.Materials)
            {
                MaterialCombo.Items.Add(mat);
            }
        }

        public void SetCollisionBounds(Bounds b)
        {
            CollisionBounds = b;
            CollisionGeom = b as BoundGeometry;
            Tag = b;
            UpdateFormTitle();
            UpdateUI();
            waschanged = b?.HasChanged ?? false;
        }

        public void UpdateFormTitleYbnChanged()
        {
            bool changed = CollisionBounds?.HasChanged ?? false;
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
            string fn = CollisionBounds?.GetTitle() ?? "untitled";
            Text = fn + ((CollisionBounds?.HasChanged??false) ? "*" : "");
        }


        public void UpdateUI()
        {
            var b = CollisionBounds;
            if (b == null)
            {
                BBMinTextBox.Text = string.Empty;
                BBMaxTextBox.Text = string.Empty;
                BBCenterTextBox.Text = string.Empty;
                BSCenterTextBox.Text = string.Empty;
                BSRadiusTextBox.Text = string.Empty;
                MarginTextBox.Text = string.Empty;
                VolumeTextBox.Text = string.Empty;
                UnkVectorTextBox.Text = string.Empty;
                MaterialColourUpDown.Value = 0;
                MaterialCombo.Text = "";
                ProceduralIDUpDown.Value = 0;
                RoomIDUpDown.Value = 0;
                PedDensityUpDown.Value = 0;
                PolyFlagsUpDown.Value = 0;
                UnkFlagsUpDown.Value = 0;
                UnkTypeUpDown.Value = 0;
                BoundsTabControl.TabPages.Remove(GeometryTabPage);
                CenterGeomTextBox.Text = string.Empty;
                QuantumTextBox.Text = string.Empty;
                UnkFloat1TextBox.Text = string.Empty;
                UnkFloat2TextBox.Text = string.Empty;
                VertexCountLabel.Text = "0 vertices";
                PolyCountLabel.Text = "0 polygons";
            }
            else
            {
                populatingui = true;

                BBMinTextBox.Text = FloatUtil.GetVector3String(b.BoxMin);
                BBMaxTextBox.Text = FloatUtil.GetVector3String(b.BoxMax);
                BBCenterTextBox.Text = FloatUtil.GetVector3String(b.BoxCenter);
                BSCenterTextBox.Text = FloatUtil.GetVector3String(b.SphereCenter);
                BSRadiusTextBox.Text = FloatUtil.ToString(b.SphereRadius);
                MarginTextBox.Text = FloatUtil.ToString(b.Margin);
                VolumeTextBox.Text = FloatUtil.ToString(b.Volume);
                UnkVectorTextBox.Text = FloatUtil.GetVector3String(b.Unknown_60h);
                MaterialColourUpDown.Value = b.MaterialColorIndex;
                MaterialCombo.SelectedIndex = b.MaterialIndex;
                ProceduralIDUpDown.Value = b.ProceduralId;
                RoomIDUpDown.Value = b.RoomId;
                PedDensityUpDown.Value = b.PedDensity;
                PolyFlagsUpDown.Value = b.PolyFlags;
                UnkFlagsUpDown.Value = b.UnkFlags;
                UnkTypeUpDown.Value = b.Unknown_3Ch;

                if (b is BoundGeometry bg)
                {
                    if (!BoundsTabControl.TabPages.Contains(GeometryTabPage))
                    {
                        BoundsTabControl.TabPages.Add(GeometryTabPage);
                    }

                    CenterGeomTextBox.Text = FloatUtil.GetVector3String(bg.CenterGeom);
                    QuantumTextBox.Text = FloatUtil.GetVector3String(bg.Quantum);
                    UnkFloat1TextBox.Text = FloatUtil.ToString(bg.Unknown_9Ch);
                    UnkFloat2TextBox.Text = FloatUtil.ToString(bg.Unknown_ACh);
                    VertexCountLabel.Text = bg.VerticesCount.ToString() + ((bg.VerticesCount == 1) ? " vertex" : " vertices");
                    PolyCountLabel.Text = bg.PolygonsCount.ToString() + ((bg.PolygonsCount == 1) ? " polygon" : " polygons");
                }
                else
                {
                    BoundsTabControl.TabPages.Remove(GeometryTabPage);
                    CenterGeomTextBox.Text = string.Empty;
                    QuantumTextBox.Text = string.Empty;
                    UnkFloat1TextBox.Text = string.Empty;
                    UnkFloat2TextBox.Text = string.Empty;
                    VertexCountLabel.Text = "0 vertices";
                    PolyCountLabel.Text = "0 polygons";
                }

                populatingui = false;
            }
        }

        private void BBMinTextBox_TextChanged(object sender, EventArgs e)
        {
            if (CollisionBounds == null) return;
            if (populatingui) return;
            var v = FloatUtil.ParseVector3String(BBMinTextBox.Text);
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CollisionBounds.BoxMin != v)
                {
                    CollisionBounds.BoxMin = v;
                    ProjectForm.SetYbnHasChanged(true);
                }
            }
        }

        private void BBMaxTextBox_TextChanged(object sender, EventArgs e)
        {
            if (CollisionBounds == null) return;
            if (populatingui) return;
            var v = FloatUtil.ParseVector3String(BBMaxTextBox.Text);
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CollisionBounds.BoxMax != v)
                {
                    CollisionBounds.BoxMax = v;
                    ProjectForm.SetYbnHasChanged(true);
                }
            }
        }

        private void BBCenterTextBox_TextChanged(object sender, EventArgs e)
        {
            if (CollisionBounds == null) return;
            if (populatingui) return;
            var v = FloatUtil.ParseVector3String(BBCenterTextBox.Text);
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CollisionBounds.BoxCenter != v)
                {
                    CollisionBounds.BoxCenter = v;
                    ProjectForm.SetYbnHasChanged(true);
                }
            }
        }

        private void BSCenterTextBox_TextChanged(object sender, EventArgs e)
        {
            if (CollisionBounds == null) return;
            if (populatingui) return;
            var v = FloatUtil.ParseVector3String(BSCenterTextBox.Text);
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CollisionBounds.SphereCenter != v)
                {
                    CollisionBounds.SphereCenter = v;
                    ProjectForm.SetYbnHasChanged(true);
                }
            }
        }

        private void BSRadiusTextBox_TextChanged(object sender, EventArgs e)
        {
            if (CollisionBounds == null) return;
            if (populatingui) return;
            var v = FloatUtil.Parse(BSRadiusTextBox.Text);
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CollisionBounds.SphereRadius != v)
                {
                    CollisionBounds.SphereRadius = v;
                    ProjectForm.SetYbnHasChanged(true);
                }
            }
        }

        private void MarginTextBox_TextChanged(object sender, EventArgs e)
        {
            if (CollisionBounds == null) return;
            if (populatingui) return;
            var v = FloatUtil.Parse(MarginTextBox.Text);
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CollisionBounds.Margin != v)
                {
                    CollisionBounds.Margin = v;
                    ProjectForm.SetYbnHasChanged(true);
                }
            }
        }

        private void VolumeTextBox_TextChanged(object sender, EventArgs e)
        {
            if (CollisionBounds == null) return;
            if (populatingui) return;
            var v = FloatUtil.Parse(VolumeTextBox.Text);
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CollisionBounds.Volume != v)
                {
                    CollisionBounds.Volume = v;
                    ProjectForm.SetYbnHasChanged(true);
                }
            }
        }

        private void UnkVectorTextBox_TextChanged(object sender, EventArgs e)
        {
            if (CollisionBounds == null) return;
            if (populatingui) return;
            var v = FloatUtil.ParseVector3String(UnkVectorTextBox.Text);
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CollisionBounds.Unknown_60h != v)
                {
                    CollisionBounds.Unknown_60h = v;
                    ProjectForm.SetYbnHasChanged(true);
                }
            }
        }

        private void MaterialCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (CollisionBounds == null) return;
            if (populatingui) return;
            var v = (byte)MaterialCombo.SelectedIndex;
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CollisionBounds.MaterialIndex != v)
                {
                    CollisionBounds.MaterialIndex = v;
                    ProjectForm.SetYbnHasChanged(true);
                }
            }
        }

        private void MaterialColourUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (CollisionBounds == null) return;
            if (populatingui) return;
            var v = (byte)MaterialColourUpDown.Value;
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CollisionBounds.MaterialColorIndex != v)
                {
                    CollisionBounds.MaterialColorIndex = v;
                    ProjectForm.SetYbnHasChanged(true);
                }
            }
        }

        private void ProceduralIDUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (CollisionBounds == null) return;
            if (populatingui) return;
            var v = (byte)ProceduralIDUpDown.Value;
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CollisionBounds.ProceduralId != v)
                {
                    CollisionBounds.ProceduralId = v;
                    ProjectForm.SetYbnHasChanged(true);
                }
            }
        }

        private void RoomIDUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (CollisionBounds == null) return;
            if (populatingui) return;
            var v = (byte)RoomIDUpDown.Value;
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CollisionBounds.RoomId != v)
                {
                    CollisionBounds.RoomId = v;
                    ProjectForm.SetYbnHasChanged(true);
                }
            }
        }

        private void PedDensityUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (CollisionBounds == null) return;
            if (populatingui) return;
            var v = (byte)PedDensityUpDown.Value;
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CollisionBounds.PedDensity != v)
                {
                    CollisionBounds.PedDensity = v;
                    ProjectForm.SetYbnHasChanged(true);
                }
            }
        }

        private void PolyFlagsUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (CollisionBounds == null) return;
            if (populatingui) return;
            var v = (byte)PolyFlagsUpDown.Value;
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CollisionBounds.PolyFlags != v)
                {
                    CollisionBounds.PolyFlags = v;
                    ProjectForm.SetYbnHasChanged(true);
                }
            }
        }

        private void UnkFlagsUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (CollisionBounds == null) return;
            if (populatingui) return;
            var v = (byte)UnkFlagsUpDown.Value;
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CollisionBounds.UnkFlags != v)
                {
                    CollisionBounds.UnkFlags = v;
                    ProjectForm.SetYbnHasChanged(true);
                }
            }
        }

        private void UnkTypeUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (CollisionBounds == null) return;
            if (populatingui) return;
            var v = (uint)UnkTypeUpDown.Value;
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CollisionBounds.Unknown_3Ch != v)
                {
                    CollisionBounds.Unknown_3Ch = v;
                    ProjectForm.SetYbnHasChanged(true);
                }
            }
        }

        private void CenterGeomTextBox_TextChanged(object sender, EventArgs e)
        {
            if (CollisionGeom == null) return;
            if (populatingui) return;
            var v = FloatUtil.ParseVector3String(CenterGeomTextBox.Text);
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CollisionGeom.CenterGeom != v)
                {
                    CollisionGeom.CenterGeom = v;
                    ProjectForm.SetYbnHasChanged(true);
                }
            }
        }

        private void QuantumTextBox_TextChanged(object sender, EventArgs e)
        {
            if (CollisionGeom == null) return;
            if (populatingui) return;
            var v = FloatUtil.ParseVector3String(QuantumTextBox.Text);
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CollisionGeom.Quantum != v)
                {
                    CollisionGeom.Quantum = v;
                    ProjectForm.SetYbnHasChanged(true);
                }
            }
        }

        private void UnkFloat1TextBox_TextChanged(object sender, EventArgs e)
        {
            if (CollisionGeom == null) return;
            if (populatingui) return;
            var v = FloatUtil.Parse(UnkFloat1TextBox.Text);
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CollisionGeom.Unknown_9Ch != v)
                {
                    CollisionGeom.Unknown_9Ch = v;
                    ProjectForm.SetYbnHasChanged(true);
                }
            }
        }

        private void UnkFloat2TextBox_TextChanged(object sender, EventArgs e)
        {
            if (CollisionGeom == null) return;
            if (populatingui) return;
            var v = FloatUtil.Parse(UnkFloat2TextBox.Text);
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CollisionGeom.Unknown_ACh != v)
                {
                    CollisionGeom.Unknown_ACh = v;
                    ProjectForm.SetYbnHasChanged(true);
                }
            }
        }
    }
}
