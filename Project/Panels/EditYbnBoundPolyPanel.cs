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
    public partial class EditYbnBoundPolyPanel : ProjectPanel
    {
        public ProjectForm ProjectForm;
        public BoundPolygon CollisionPoly { get; set; }
        public BoundPolygonTriangle CollisionTriangle { get; set; }
        public BoundPolygonSphere CollisionSphere { get; set; }
        public BoundPolygonCapsule CollisionCapsule { get; set; }
        public BoundPolygonBox CollisionBox { get; set; }
        public BoundPolygonCylinder CollisionCylinder { get; set; }

        private bool populatingui = false;
        private bool waschanged = false;

        public EditYbnBoundPolyPanel(ProjectForm projectForm)
        {
            ProjectForm = projectForm;
            InitializeComponent();

            LoadDropDowns();
        }

        private void LoadDropDowns()
        {
            MatTypeCombo.Items.Clear();
            if (BoundsMaterialTypes.Materials == null) return;
            foreach (var mat in BoundsMaterialTypes.Materials)
            {
                MatTypeCombo.Items.Add(mat);
            }
        }

        public void SetCollisionPoly(BoundPolygon b)
        {
            CollisionPoly = b;
            CollisionTriangle = b as BoundPolygonTriangle;
            CollisionSphere = b as BoundPolygonSphere;
            CollisionCapsule = b as BoundPolygonCapsule;
            CollisionBox = b as BoundPolygonBox;
            CollisionCylinder = b as BoundPolygonCylinder;
            Tag = b;
            UpdateFormTitle();
            UpdateUI();
            waschanged = b?.Owner?.HasChanged ?? false;
        }

        public void UpdateFormTitleYbnChanged()
        {
            bool changed = CollisionPoly?.Owner?.HasChanged ?? false;
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
            string fn = CollisionPoly?.Title ?? "untitled";
            Text = fn + ((CollisionPoly?.Owner?.HasChanged ?? false) ? "*" : "");
        }


        public void UpdateUI()
        {
            if (CollisionPoly == null)
            {
                AddToProjectButton.Enabled = false;
                DeleteButton.Enabled = false;
                PolyTabControl.TabPages.Clear();
                TriVertex1TextBox.Text = string.Empty;
                TriVertex2TextBox.Text = string.Empty;
                TriVertex3TextBox.Text = string.Empty;
                TriAreaTextBox.Text = string.Empty;
                TriEdge1UpDown.Value = 0;
                TriEdge2UpDown.Value = 0;
                TriEdge3UpDown.Value = 0;
                TriFlag1CheckBox.Checked = false;
                TriFlag2CheckBox.Checked = false;
                TriFlag3CheckBox.Checked = false;
                SphPositionTextBox.Text = string.Empty;
                SphRadiusTextBox.Text = string.Empty;
                CapVertex1TextBox.Text = string.Empty;
                CapVertex2TextBox.Text = string.Empty;
                CapRadiusTextBox.Text = string.Empty;
                BoxVertex1TextBox.Text = string.Empty;
                BoxVertex2TextBox.Text = string.Empty;
                BoxVertex3TextBox.Text = string.Empty;
                BoxVertex4TextBox.Text = string.Empty;
                CylVertex1TextBox.Text = string.Empty;
                CylVertex2TextBox.Text = string.Empty;
                CylRadiusTextBox.Text = string.Empty;
                MatTypeCombo.Text = string.Empty;
                MatColourUpDown.Value = 0;
                MatProceduralIDUpDown.Value = 0;
                MatRoomIDUpDown.Value = 0;
                MatPedDensityUpDown.Value = 0;
                MatUnkUpDown.Value = 0;
                SetCheckedListBoxValues(MatFlagsCheckedListBox, 0);
            }
            else
            {
                populatingui = true;

                if (CollisionTriangle != null)
                {
                    TriVertex1TextBox.Text = FloatUtil.GetVector3String(CollisionTriangle.Vertex1);
                    TriVertex2TextBox.Text = FloatUtil.GetVector3String(CollisionTriangle.Vertex2);
                    TriVertex3TextBox.Text = FloatUtil.GetVector3String(CollisionTriangle.Vertex3);
                    TriAreaTextBox.Text = FloatUtil.ToString(CollisionTriangle.triArea);
                    TriEdge1UpDown.Value = CollisionTriangle.edgeIndex1;
                    TriEdge2UpDown.Value = CollisionTriangle.edgeIndex2;
                    TriEdge3UpDown.Value = CollisionTriangle.edgeIndex3;
                    TriFlag1CheckBox.Checked = CollisionTriangle.vertFlag1;
                    TriFlag2CheckBox.Checked = CollisionTriangle.vertFlag2;
                    TriFlag3CheckBox.Checked = CollisionTriangle.vertFlag3;
                    if (!PolyTabControl.TabPages.Contains(TriangleTabPage)) PolyTabControl.TabPages.Add(TriangleTabPage);
                }
                else
                {
                    PolyTabControl.TabPages.Remove(TriangleTabPage);
                }

                if (CollisionSphere != null)
                {
                    SphPositionTextBox.Text = FloatUtil.GetVector3String(CollisionSphere.Position);
                    SphRadiusTextBox.Text = FloatUtil.ToString(CollisionSphere.sphereRadius);
                    if (!PolyTabControl.TabPages.Contains(SphereTabPage)) PolyTabControl.TabPages.Add(SphereTabPage);
                }
                else
                {
                    PolyTabControl.TabPages.Remove(SphereTabPage);
                }

                if (CollisionCapsule != null)
                {
                    CapVertex1TextBox.Text = FloatUtil.GetVector3String(CollisionCapsule.Vertex1);
                    CapVertex2TextBox.Text = FloatUtil.GetVector3String(CollisionCapsule.Vertex2);
                    CapRadiusTextBox.Text = FloatUtil.ToString(CollisionCapsule.capsuleRadius);
                    if (!PolyTabControl.TabPages.Contains(CapsuleTabPage)) PolyTabControl.TabPages.Add(CapsuleTabPage);
                }
                else
                {
                    PolyTabControl.TabPages.Remove(CapsuleTabPage);
                }

                if (CollisionBox != null)
                {
                    BoxVertex1TextBox.Text = FloatUtil.GetVector3String(CollisionBox.Vertex1);
                    BoxVertex2TextBox.Text = FloatUtil.GetVector3String(CollisionBox.Vertex2);
                    BoxVertex3TextBox.Text = FloatUtil.GetVector3String(CollisionBox.Vertex3);
                    BoxVertex4TextBox.Text = FloatUtil.GetVector3String(CollisionBox.Vertex4);
                    if (!PolyTabControl.TabPages.Contains(BoxTabPage)) PolyTabControl.TabPages.Add(BoxTabPage);
                }
                else
                {
                    PolyTabControl.TabPages.Remove(BoxTabPage);
                }

                if (CollisionCylinder != null)
                {
                    CylVertex1TextBox.Text = FloatUtil.GetVector3String(CollisionCylinder.Vertex1);
                    CylVertex2TextBox.Text = FloatUtil.GetVector3String(CollisionCylinder.Vertex2);
                    CylRadiusTextBox.Text = FloatUtil.ToString(CollisionCylinder.cylinderRadius);
                    if (!PolyTabControl.TabPages.Contains(CylinderTabPage)) PolyTabControl.TabPages.Add(CylinderTabPage);
                }
                else
                {
                    PolyTabControl.TabPages.Remove(CylinderTabPage);
                }

                var m = CollisionPoly.Material;
                MatTypeCombo.SelectedIndex = m.Type.Index;
                MatColourUpDown.Value = m.MaterialColorIndex;
                MatProceduralIDUpDown.Value = m.ProceduralId;
                MatRoomIDUpDown.Value = m.RoomId;
                MatPedDensityUpDown.Value = m.PedDensity;
                MatUnkUpDown.Value = m.Unk4;
                SetCheckedListBoxValues(MatFlagsCheckedListBox, (ushort)m.Flags);

                var ybn = CollisionPoly.Owner?.GetRootYbn();
                AddToProjectButton.Enabled = (ybn != null) ? !ProjectForm.YbnExistsInProject(ybn) : false;
                DeleteButton.Enabled = !AddToProjectButton.Enabled;

                populatingui = false;
            }
        }

        private void SetCheckedListBoxValues(CheckedListBox clb, uint flags)
        {
            for (int i = 0; i < clb.Items.Count; i++)
            {
                var c = ((flags & (1 << i)) > 0);
                clb.SetItemCheckState(i, c ? CheckState.Checked : CheckState.Unchecked);
            }
        }
        private uint GetCheckedListBoxValues(CheckedListBox clb, ItemCheckEventArgs e)
        {
            uint r = 0;
            for (int i = 0; i < clb.Items.Count; i++)
            {
                if ((e != null) && (e.Index == i))
                {
                    if (e.NewValue == CheckState.Checked)
                    {
                        r += (uint)(1 << i);
                    }
                }
                else
                {
                    bool v = clb.GetItemChecked(i);// == CheckState.Checked;
                    r = BitUtil.UpdateBit(r, i, v);
                }
            }
            return r;
        }


        private void UpdatePolyMaterial(BoundMaterial_s mat)
        {
            if (CollisionPoly == null) return;
            var shared = UpdateSharedMaterialCheckBox.Checked && (CollisionPoly.Owner != null);
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (shared)
                {
                    CollisionPoly.Owner.SetMaterial(CollisionPoly.Index, mat);
                }
                else
                {
                    CollisionPoly.Material = mat;
                }
                ProjectForm.SetYbnHasChanged(true);
            }
            if ((ProjectForm.WorldForm != null) && (CollisionPoly.Owner != null))
            {
                ProjectForm.WorldForm.UpdateCollisionBoundsGraphics(CollisionPoly.Owner);
            }
        }



        private void TriVertex1TextBox_TextChanged(object sender, EventArgs e)
        {
            if (CollisionTriangle == null) return;
            if (populatingui) return;
            var v = FloatUtil.ParseVector3String(TriVertex1TextBox.Text);
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CollisionTriangle.Vertex1 != v)
                {
                    CollisionTriangle.Vertex1 = v;
                    ProjectForm.SetYbnHasChanged(true);
                }
            }
        }

        private void TriVertex2TextBox_TextChanged(object sender, EventArgs e)
        {
            if (CollisionTriangle == null) return;
            if (populatingui) return;
            var v = FloatUtil.ParseVector3String(TriVertex2TextBox.Text);
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CollisionTriangle.Vertex2 != v)
                {
                    CollisionTriangle.Vertex2 = v;
                    ProjectForm.SetYbnHasChanged(true);
                }
            }
        }

        private void TriVertex3TextBox_TextChanged(object sender, EventArgs e)
        {
            if (CollisionTriangle == null) return;
            if (populatingui) return;
            var v = FloatUtil.ParseVector3String(TriVertex3TextBox.Text);
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CollisionTriangle.Vertex3 != v)
                {
                    CollisionTriangle.Vertex3 = v;
                    ProjectForm.SetYbnHasChanged(true);
                }
            }
        }

        private void TriAreaTextBox_TextChanged(object sender, EventArgs e)
        {
            if (CollisionTriangle == null) return;
            if (populatingui) return;
            var v = FloatUtil.Parse(TriAreaTextBox.Text);
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CollisionTriangle.triArea != v)
                {
                    CollisionTriangle.triArea = v;
                    ProjectForm.SetYbnHasChanged(true);
                }
            }
        }

        private void TriEdge1UpDown_ValueChanged(object sender, EventArgs e)
        {
            if (CollisionTriangle == null) return;
            if (populatingui) return;
            var v = (short)TriEdge1UpDown.Value;
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CollisionTriangle.edgeIndex1 != v)
                {
                    CollisionTriangle.edgeIndex1 = v;
                    ProjectForm.SetYbnHasChanged(true);
                }
            }
        }

        private void TriEdge2UpDown_ValueChanged(object sender, EventArgs e)
        {
            if (CollisionTriangle == null) return;
            if (populatingui) return;
            var v = (short)TriEdge2UpDown.Value;
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CollisionTriangle.edgeIndex2 != v)
                {
                    CollisionTriangle.edgeIndex2 = v;
                    ProjectForm.SetYbnHasChanged(true);
                }
            }
        }

        private void TriEdge3UpDown_ValueChanged(object sender, EventArgs e)
        {
            if (CollisionTriangle == null) return;
            if (populatingui) return;
            var v = (short)TriEdge3UpDown.Value;
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CollisionTriangle.edgeIndex3 != v)
                {
                    CollisionTriangle.edgeIndex3 = v;
                    ProjectForm.SetYbnHasChanged(true);
                }
            }
        }

        private void TriFlag1CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (CollisionTriangle == null) return;
            if (populatingui) return;
            var v = TriFlag1CheckBox.Checked;
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CollisionTriangle.vertFlag1 != v)
                {
                    CollisionTriangle.vertFlag1 = v;
                    ProjectForm.SetYbnHasChanged(true);
                }
            }
        }

        private void TriFlag2CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (CollisionTriangle == null) return;
            if (populatingui) return;
            var v = TriFlag2CheckBox.Checked;
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CollisionTriangle.vertFlag2 != v)
                {
                    CollisionTriangle.vertFlag2 = v;
                    ProjectForm.SetYbnHasChanged(true);
                }
            }
        }

        private void TriFlag3CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (CollisionTriangle == null) return;
            if (populatingui) return;
            var v = TriFlag3CheckBox.Checked;
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CollisionTriangle.vertFlag3 != v)
                {
                    CollisionTriangle.vertFlag3 = v;
                    ProjectForm.SetYbnHasChanged(true);
                }
            }
        }

        private void SphPositionTextBox_TextChanged(object sender, EventArgs e)
        {
            if (CollisionSphere == null) return;
            if (populatingui) return;
            var v = FloatUtil.ParseVector3String(SphPositionTextBox.Text);
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CollisionSphere.Position != v)
                {
                    CollisionSphere.Position = v;
                    ProjectForm.SetYbnHasChanged(true);
                }
            }
        }

        private void SphRadiusTextBox_TextChanged(object sender, EventArgs e)
        {
            if (CollisionSphere == null) return;
            if (populatingui) return;
            var v = FloatUtil.Parse(SphRadiusTextBox.Text);
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CollisionSphere.sphereRadius != v)
                {
                    CollisionSphere.sphereRadius = v;
                    ProjectForm.SetYbnHasChanged(true);
                }
            }
        }

        private void CapVertex1TextBox_TextChanged(object sender, EventArgs e)
        {
            if (CollisionCapsule == null) return;
            if (populatingui) return;
            var v = FloatUtil.ParseVector3String(CapVertex1TextBox.Text);
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CollisionCapsule.Vertex1 != v)
                {
                    CollisionCapsule.Vertex1 = v;
                    ProjectForm.SetYbnHasChanged(true);
                }
            }
        }

        private void CapVertex2TextBox_TextChanged(object sender, EventArgs e)
        {
            if (CollisionCapsule == null) return;
            if (populatingui) return;
            var v = FloatUtil.ParseVector3String(CapVertex2TextBox.Text);
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CollisionCapsule.Vertex2 != v)
                {
                    CollisionCapsule.Vertex2 = v;
                    ProjectForm.SetYbnHasChanged(true);
                }
            }
        }

        private void CapRadiusTextBox_TextChanged(object sender, EventArgs e)
        {
            if (CollisionCapsule == null) return;
            if (populatingui) return;
            var v = FloatUtil.Parse(CapRadiusTextBox.Text);
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CollisionCapsule.capsuleRadius != v)
                {
                    CollisionCapsule.capsuleRadius = v;
                    ProjectForm.SetYbnHasChanged(true);
                }
            }
        }

        private void BoxVertex1TextBox_TextChanged(object sender, EventArgs e)
        {
            if (CollisionBox == null) return;
            if (populatingui) return;
            var v = FloatUtil.ParseVector3String(BoxVertex1TextBox.Text);
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CollisionBox.Vertex1 != v)
                {
                    CollisionBox.Vertex1 = v;
                    ProjectForm.SetYbnHasChanged(true);
                }
            }
        }

        private void BoxVertex2TextBox_TextChanged(object sender, EventArgs e)
        {
            if (CollisionBox == null) return;
            if (populatingui) return;
            var v = FloatUtil.ParseVector3String(BoxVertex2TextBox.Text);
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CollisionBox.Vertex2 != v)
                {
                    CollisionBox.Vertex2 = v;
                    ProjectForm.SetYbnHasChanged(true);
                }
            }
        }

        private void BoxVertex3TextBox_TextChanged(object sender, EventArgs e)
        {
            if (CollisionBox == null) return;
            if (populatingui) return;
            var v = FloatUtil.ParseVector3String(BoxVertex3TextBox.Text);
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CollisionBox.Vertex3 != v)
                {
                    CollisionBox.Vertex3 = v;
                    ProjectForm.SetYbnHasChanged(true);
                }
            }
        }

        private void BoxVertex4TextBox_TextChanged(object sender, EventArgs e)
        {
            if (CollisionBox == null) return;
            if (populatingui) return;
            var v = FloatUtil.ParseVector3String(BoxVertex4TextBox.Text);
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CollisionBox.Vertex4 != v)
                {
                    CollisionBox.Vertex4 = v;
                    ProjectForm.SetYbnHasChanged(true);
                }
            }
        }

        private void CylVertex1TextBox_TextChanged(object sender, EventArgs e)
        {
            if (CollisionCylinder == null) return;
            if (populatingui) return;
            var v = FloatUtil.ParseVector3String(CylVertex1TextBox.Text);
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CollisionCylinder.Vertex1 != v)
                {
                    CollisionCylinder.Vertex1 = v;
                    ProjectForm.SetYbnHasChanged(true);
                }
            }
        }

        private void CylVertex2TextBox_TextChanged(object sender, EventArgs e)
        {
            if (CollisionCylinder == null) return;
            if (populatingui) return;
            var v = FloatUtil.ParseVector3String(CylVertex2TextBox.Text);
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CollisionCylinder.Vertex2 != v)
                {
                    CollisionCylinder.Vertex2 = v;
                    ProjectForm.SetYbnHasChanged(true);
                }
            }
        }

        private void CylRadiusTextBox_TextChanged(object sender, EventArgs e)
        {
            if (CollisionCylinder == null) return;
            if (populatingui) return;
            var v = FloatUtil.Parse(CylRadiusTextBox.Text);
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CollisionCylinder.cylinderRadius != v)
                {
                    CollisionCylinder.cylinderRadius = v;
                    ProjectForm.SetYbnHasChanged(true);
                }
            }
        }

        private void MatTypeCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (CollisionPoly == null) return;
            if (populatingui) return;
            var mat = CollisionPoly.Material;
            var v = (byte)MatTypeCombo.SelectedIndex;
            if (mat.Type != v)
            {
                mat.Type = v;
                UpdatePolyMaterial(mat);
            }
        }

        private void MatColourUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (CollisionPoly == null) return;
            if (populatingui) return;
            var mat = CollisionPoly.Material;
            var v = (byte)MatColourUpDown.Value;
            if (mat.MaterialColorIndex != v)
            {
                mat.MaterialColorIndex = v;
                UpdatePolyMaterial(mat);
            }
        }

        private void MatProceduralIDUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (CollisionPoly == null) return;
            if (populatingui) return;
            var mat = CollisionPoly.Material;
            var v = (byte)MatProceduralIDUpDown.Value;
            if (mat.ProceduralId != v)
            {
                mat.ProceduralId = v;
                UpdatePolyMaterial(mat);
            }
        }

        private void MatRoomIDUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (CollisionPoly == null) return;
            if (populatingui) return;
            var mat = CollisionPoly.Material;
            var v = (byte)MatRoomIDUpDown.Value;
            if (mat.RoomId != v)
            {
                mat.RoomId = v;
                UpdatePolyMaterial(mat);
            }
        }

        private void MatPedDensityUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (CollisionPoly == null) return;
            if (populatingui) return;
            var mat = CollisionPoly.Material;
            var v = (byte)MatPedDensityUpDown.Value;
            if (mat.PedDensity != v)
            {
                mat.PedDensity = v;
                UpdatePolyMaterial(mat);
            }
        }

        private void MatUnkUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (CollisionPoly == null) return;
            if (populatingui) return;
            var mat = CollisionPoly.Material;
            var v = (ushort)MatUnkUpDown.Value;
            if (mat.Unk4 != v)
            {
                mat.Unk4 = v;
                UpdatePolyMaterial(mat);
            }
        }

        private void MatFlagsCheckedListBox_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (CollisionPoly == null) return;
            if (populatingui) return;
            var mat = CollisionPoly.Material;
            var v = (EBoundMaterialFlags)GetCheckedListBoxValues(MatFlagsCheckedListBox, e);
            if (mat.Flags != v)
            {
                mat.Flags = v;
                UpdatePolyMaterial(mat);
            }
        }

        private void AddToProjectButton_Click(object sender, EventArgs e)
        {
            ProjectForm.SetProjectItem(CollisionPoly);
            ProjectForm.AddCollisionPolyToProject();
        }

        private void DeleteButton_Click(object sender, EventArgs e)
        {
            ProjectForm.SetProjectItem(CollisionPoly);
            ProjectForm.DeleteCollisionPoly();
        }
    }
}
