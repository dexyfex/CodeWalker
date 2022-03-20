using CodeWalker.GameFiles;
using CodeWalker.World;
using SharpDX;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CodeWalker.Forms
{
    public partial class ModelLightForm : Form
    {
        private ModelForm ModelForm;
        private Drawable Drawable;
        private FragDrawable FragDrawable;
        private Dictionary<uint, Drawable> DrawableDict;

        LightAttributes selectedLight = null;

        bool populatingui = false;
        
        public ModelLightForm(ModelForm modelForm)
        {
            InitializeComponent();

            ModelForm = modelForm;
        }

        public void LoadModel(DrawableBase drawable)
        {
            LightsTreeView.Nodes.Clear();
            LightsTreeView.ShowRootLines = false;

            var dr = drawable as Drawable;
            var fr = drawable as FragDrawable;

            if (dr != null)
            {
                Drawable = dr;
                var lights = dr.LightAttributes;
                if (lights != null)
                {
                    AddLightsTreeNodes(lights.data_items);
                }
            }
            else if (fr != null)
            {
                FragDrawable = fr;
                var lights = fr.OwnerFragment?.LightAttributes;
                if (lights != null)
                {
                    AddLightsTreeNodes(lights.data_items);
                }
            }
        }
        public void LoadModels(Dictionary<uint, Drawable> dict)
        {
            DrawableDict = dict;

            LightsTreeView.Nodes.Clear();

            if (dict != null)
            {
                foreach (var kvp in dict)
                {
                    MetaHash mhash = new MetaHash(kvp.Key);
                    var drawable = kvp.Value;

                    var dnode = LightsTreeView.Nodes.Add(mhash.ToString());
                    dnode.Tag = drawable;

                    var lights = drawable.LightAttributes.data_items;
                    if(lights != null)
                    {
                        AddLightsTreeNodes(lights, dnode);
                    }

                    dnode.Expand();
                }
            }
        }
        private void AddLightsTreeNodes(LightAttributes[] lights, TreeNode parent = null)
        {
            if (lights == null) return;

            for (int mi = 0; mi < lights.Length; mi++)
            {
                var tnc = LightsTreeView.Nodes;
                var light = lights[mi];
                string mprefix = "Light" + (mi + 1).ToString() + " : " + light.Type.ToString();
                var mnode = parent == null ? tnc.Add(mprefix) : parent.Nodes.Add(mprefix);
                mnode.Tag = light;
                mnode.Expand();
            }
        }



        
        public void UpdateUI()
        {
            var light = selectedLight;

            if(light == null)
            {
                DeleteLightButton.Enabled = false;
                EditDeleteLightMenu.Enabled = false;
                EditDuplicateLightMenu.Enabled = false;
                populatingui = true;
                PositionTextBox.Text = "";
                DirectionTextBox.Text = "";
                TangentTextBox.Text = "";
                TypeComboBox.SelectedItem = "Point";
                ColourRUpDown.Value = 0;
                ColourGUpDown.Value = 0;
                ColourBUpDown.Value = 0;
                IntensityTextBox.Text = "";
                FlagsTextBox.Text = "";
                FlashinessUpDown.Value = 0;
                BoneIDUpDown.Value = 0;
                GroupIDUpDown.Value = 0;
                FalloffTextBox.Text = "";
                FalloffExponentTextBox.Text = "";
                InnerAngleTextBox.Text = "";
                OuterAngleTextBox.Text = "";
                CoronaSizeTextBox.Text = "";
                CoronaIntensityTextBox.Text = "";
                ExtentTextBox.Text = "";
                ShadowBlurUpDown.Value = 0;
                LightFadeDistanceUpDown.Value = 0;
                CoronaZBiasTextBox.Text = "";
                TextureHashTextBox.Text = "";
                VolumeIntensityTextBox.Text = "";
                VolumeSizeScaleTextBox.Text = "";
                VolumeColorRUpDown.Value = 0;
                VolumeColorGUpDown.Value = 0;
                VolumeColorBUpDown.Value = 0;
                VolumeOuterExponentTextBox.Text = "";
                ShadowFadeDistanceUpDown.Value = 0;
                SpecularFadeDistanceUpDown.Value = 0;
                VolumetricFadeDistanceUpDown.Value = 0;
                ShadowNearClipTextBox.Text = "";
                CullingPlaneNormalTextBox.Text = "";
                CullingPlaneOffsetTextBox.Text = "";
                TimeFlagsTextBox.Text = "";
                populatingui = false;
            }
            else
            {
                DeleteLightButton.Enabled = true;
                EditDeleteLightMenu.Enabled = true;
                EditDuplicateLightMenu.Enabled = true;
                populatingui = true;
                PositionTextBox.Text = FloatUtil.GetVector3String(light.Position);
                DirectionTextBox.Text = FloatUtil.GetVector3String(light.Direction);
                TangentTextBox.Text = FloatUtil.GetVector3String(light.Tangent);
                TypeComboBox.SelectedItem = light.Type.ToString();
                ColourRUpDown.Value = light.ColorR;
                ColourGUpDown.Value = light.ColorG;
                ColourBUpDown.Value = light.ColorB;
                ColourLabel.BackColor = System.Drawing.Color.FromArgb(light.ColorR, light.ColorG, light.ColorB);
                IntensityTextBox.Text = FloatUtil.ToString(light.Intensity);
                FlagsTextBox.Text = light.Flags.ToString();
                FlashinessUpDown.Value = light.Flashiness;
                BoneIDUpDown.Value = light.BoneId;
                GroupIDUpDown.Value = light.GroupId;
                FalloffTextBox.Text = FloatUtil.ToString(light.Falloff);
                FalloffExponentTextBox.Text = FloatUtil.ToString(light.FalloffExponent);
                InnerAngleTextBox.Text = FloatUtil.ToString(light.ConeInnerAngle);
                OuterAngleTextBox.Text = FloatUtil.ToString(light.ConeOuterAngle);
                CoronaSizeTextBox.Text = FloatUtil.ToString(light.CoronaSize);
                CoronaIntensityTextBox.Text = FloatUtil.ToString(light.CoronaIntensity);
                ExtentTextBox.Text = FloatUtil.GetVector3String(light.Extent);
                ShadowBlurUpDown.Value = light.ShadowBlur;
                LightFadeDistanceUpDown.Value = light.LightFadeDistance;
                CoronaZBiasTextBox.Text = FloatUtil.ToString(light.CoronaZBias);
                TextureHashTextBox.Text = light.ProjectedTextureHash.ToCleanString();
                VolumeIntensityTextBox.Text = FloatUtil.ToString(light.VolumeIntensity);
                VolumeSizeScaleTextBox.Text = FloatUtil.ToString(light.VolumeSizeScale);
                VolumeColorRUpDown.Value = light.VolumeOuterColorR;
                VolumeColorGUpDown.Value = light.VolumeOuterColorG;
                VolumeColorBUpDown.Value = light.VolumeOuterColorB;
                VolumeColorLabel.BackColor = System.Drawing.Color.FromArgb(light.VolumeOuterColorR, light.VolumeOuterColorG, light.VolumeOuterColorB);
                VolumeOuterExponentTextBox.Text = FloatUtil.ToString(light.VolumeOuterExponent);
                ShadowFadeDistanceUpDown.Value = light.ShadowFadeDistance;
                SpecularFadeDistanceUpDown.Value = light.SpecularFadeDistance;
                VolumetricFadeDistanceUpDown.Value = light.VolumetricFadeDistance;
                ShadowNearClipTextBox.Text = FloatUtil.ToString(light.ShadowNearClip);
                CullingPlaneNormalTextBox.Text = FloatUtil.GetVector3String(light.CullingPlaneNormal);
                CullingPlaneOffsetTextBox.Text = FloatUtil.ToString(light.CullingPlaneOffset);
                TimeFlagsTextBox.Text = light.TimeFlags.ToString();
                UpdateFlagsCheckBoxes();
                populatingui = false;
            }
        }

        public void UpdateLightParams()
        {
            if (selectedLight == null) return;
            selectedLight.UpdateRenderable = true;
        }


        private LightAttributes NewLightAttribute()
        {
            LightAttributes light = new LightAttributes();
            light.Direction = Vector3.BackwardLH;
            light.Tangent = Vector3.Right;
            light.Intensity = 5;
            light.ConeInnerAngle = 5;
            light.ConeOuterAngle = 35;
            light.Extent = Vector3.One;
            light.Type = LightType.Spot;
            light.Falloff = 10;
            light.ColorR = 255;
            light.ColorG = 255;
            light.ColorB = 255;
            light.TimeFlags = 14680191;
            return light;
        }
        private LightAttributes DuplicateLightAttribute()
        {
            LightAttributes light = new LightAttributes();
            light.Unknown_0h = selectedLight.Unknown_0h;
            light.Unknown_4h = selectedLight.Unknown_4h;
            light.Position = selectedLight.Position;
            light.Unknown_14h = selectedLight.Unknown_14h;
            light.ColorR = selectedLight.ColorR;
            light.ColorG = selectedLight.ColorG;
            light.ColorB = selectedLight.ColorB;
            light.Flashiness = selectedLight.Flashiness;
            light.Intensity = selectedLight.Intensity;
            light.Flags = selectedLight.Flags;
            light.BoneId = selectedLight.BoneId;
            light.Type = selectedLight.Type;
            light.GroupId = selectedLight.GroupId;
            light.TimeFlags = selectedLight.TimeFlags;
            light.Falloff = selectedLight.Falloff;
            light.FalloffExponent = selectedLight.FalloffExponent;
            light.CullingPlaneNormal = selectedLight.CullingPlaneNormal;
            light.CullingPlaneOffset = selectedLight.CullingPlaneOffset;
            light.ShadowBlur = selectedLight.ShadowBlur;
            light.Unknown_45h = selectedLight.Unknown_45h;
            light.Unknown_46h = selectedLight.Unknown_46h;
            light.VolumeIntensity = selectedLight.VolumeIntensity;
            light.VolumeSizeScale = selectedLight.VolumeSizeScale;
            light.VolumeOuterColorR = selectedLight.VolumeOuterColorR;
            light.VolumeOuterColorG = selectedLight.VolumeOuterColorG;
            light.VolumeOuterColorB = selectedLight.VolumeOuterColorB;
            light.LightHash = selectedLight.LightHash;
            light.VolumeOuterIntensity = selectedLight.VolumeOuterIntensity;
            light.CoronaSize = selectedLight.CoronaSize;
            light.VolumeOuterExponent = selectedLight.VolumeOuterExponent;
            light.LightFadeDistance = selectedLight.LightFadeDistance;
            light.ShadowFadeDistance = selectedLight.ShadowFadeDistance;
            light.SpecularFadeDistance = selectedLight.SpecularFadeDistance;
            light.VolumetricFadeDistance = selectedLight.VolumetricFadeDistance;
            light.ShadowNearClip = selectedLight.ShadowNearClip;
            light.CoronaIntensity = selectedLight.CoronaIntensity;
            light.CoronaZBias = selectedLight.CoronaZBias;
            light.Direction = selectedLight.Direction;
            light.Tangent = selectedLight.Tangent;
            light.ConeInnerAngle = selectedLight.ConeInnerAngle;
            light.ConeOuterAngle = selectedLight.ConeOuterAngle;
            light.Extent = selectedLight.Extent;
            light.ProjectedTextureHash = selectedLight.ProjectedTextureHash;
            light.Unknown_A4h = selectedLight.Unknown_A4h;
           
            return light;
        }

        private void SelectLight(LightAttributes light)
        {
            if (light == null)
            {
                selectedLight = null;
                ModelForm.selectedLight = null;
                UpdateUI();
            }
            else
            {
                selectedLight = light;
                ModelForm.selectedLight = light;
                UpdateUI();

                var pos = light.Position;
                Bone bone = null;
                ModelForm.Skeleton?.BonesMap?.TryGetValue(light.BoneId, out bone);
                if (bone != null)
                {
                    var xform = bone.AbsTransform;
                    pos = xform.Multiply(pos);
                    //TODO:? handle bone's rotation correctly for widget??
                }

                ModelForm.SetWidgetTransform(pos, light.Orientation, new Vector3(light.Falloff));
            }
        }
        private void SelectLightTreeNode(LightAttributes light)
        {
            foreach (TreeNode rn in LightsTreeView.Nodes)
            {
                if (rn.Tag == light)
                {
                    LightsTreeView.SelectedNode = rn;
                    break;
                }
                var found = false;
                foreach (TreeNode tn in rn.Nodes)
                {
                    if (tn.Tag == light)
                    {
                        LightsTreeView.SelectedNode = tn;
                        found = true;
                        break;
                    }
                }
                if (found)
                {
                    break;
                }
            }
        }
        private void CreateLight()
        {
            selectedLight = NewLightAttribute();
            if(Drawable != null)
            {
                if (Drawable.LightAttributes == null) Drawable.LightAttributes = new ResourceSimpleList64<LightAttributes>();
                List<LightAttributes> lights = Drawable.LightAttributes.data_items?.ToList() ?? new List<LightAttributes>();
                lights.Add(selectedLight);
                Drawable.LightAttributes.data_items = lights.ToArray();
                UpdateLightParams();
                LoadModel(Drawable);
            }
            else if(FragDrawable != null)
            {
                if (FragDrawable.OwnerFragment.LightAttributes == null) FragDrawable.OwnerFragment.LightAttributes = new ResourceSimpleList64<LightAttributes>();
                List<LightAttributes> lights = FragDrawable.OwnerFragment.LightAttributes.data_items?.ToList() ?? new List<LightAttributes>();
                lights.Add(selectedLight);
                FragDrawable.OwnerFragment.LightAttributes.data_items = lights.ToArray();
                UpdateLightParams();
                LoadModel(FragDrawable);
            }
            else
            {
                var n = LightsTreeView.SelectedNode;
                if (n != null)
                {
                    var dr = n.Tag as Drawable;
                    if (dr == null) { dr = n.Parent.Tag as Drawable; } //try parent node tag also
                    if (dr!= null)
                    {
                        if (dr.LightAttributes == null) dr.LightAttributes = new ResourceSimpleList64<LightAttributes>();
                        List<LightAttributes> lights = dr.LightAttributes.data_items?.ToList() ?? new List<LightAttributes>();
                        lights.Add(selectedLight);
                        dr.LightAttributes.data_items = lights.ToArray();
                        UpdateLightParams();
                        LoadModels(DrawableDict);
                    }
                }
            }
            SelectLightTreeNode(selectedLight);
        }
        private void DeleteLight()
        {
            if (selectedLight == null) return;
            if (Drawable != null)
            {
                List<LightAttributes> lights = Drawable.LightAttributes.data_items.ToList();
                lights.Remove(selectedLight);
                Drawable.LightAttributes.data_items = lights.ToArray();
                UpdateLightParams();
                LoadModel(Drawable);
            }
            else if(FragDrawable != null)
            {
                List<LightAttributes> lights = FragDrawable.OwnerFragment.LightAttributes.data_items.ToList();
                lights.Remove(selectedLight);
                FragDrawable.OwnerFragment.LightAttributes.data_items = lights.ToArray();
                UpdateLightParams();
                LoadModel(Drawable);
            }
            else
            {
                var dr = LightsTreeView.SelectedNode.Parent.Tag as Drawable;
                if (dr != null)
                {
                    List<LightAttributes> lights = dr.LightAttributes.data_items.ToList();
                    lights.Remove(selectedLight);
                    dr.LightAttributes.data_items = lights.ToArray();
                    UpdateLightParams();
                    LoadModels(DrawableDict);
                }
            }

        }

        private void DuplicateLight()
        {
            if (selectedLight == null) return;
            selectedLight = DuplicateLightAttribute();
            if (Drawable != null)
            {
                if (Drawable.LightAttributes == null) Drawable.LightAttributes = new ResourceSimpleList64<LightAttributes>();
                List<LightAttributes> lights = Drawable.LightAttributes.data_items?.ToList() ?? new List<LightAttributes>();
                lights.Add(selectedLight);
                Drawable.LightAttributes.data_items = lights.ToArray();
                UpdateLightParams();
                LoadModel(Drawable);
            }
            else if (FragDrawable != null)
            {
                if (FragDrawable.OwnerFragment.LightAttributes == null) FragDrawable.OwnerFragment.LightAttributes = new ResourceSimpleList64<LightAttributes>();
                List<LightAttributes> lights = FragDrawable.OwnerFragment.LightAttributes.data_items?.ToList() ?? new List<LightAttributes>();
                lights.Add(selectedLight);
                FragDrawable.OwnerFragment.LightAttributes.data_items = lights.ToArray();
                UpdateLightParams();
                LoadModel(FragDrawable);
            }
            else
            {
                var n = LightsTreeView.SelectedNode;
                if (n != null)
                {
                    var dr = n.Tag as Drawable;
                    if (dr == null) { dr = n.Parent.Tag as Drawable; } //try parent node tag also
                    if (dr != null)
                    {
                        if (dr.LightAttributes == null) dr.LightAttributes = new ResourceSimpleList64<LightAttributes>();
                        List<LightAttributes> lights = dr.LightAttributes.data_items?.ToList() ?? new List<LightAttributes>();
                        lights.Add(selectedLight);
                        dr.LightAttributes.data_items = lights.ToArray();
                        UpdateLightParams();
                        LoadModels(DrawableDict);
                    }
                }
            }
            SelectLightTreeNode(selectedLight);
        }

        private void UpdateFlagsCheckBoxes()
        {
            var l = selectedLight;
            var tfam = (l.TimeFlags >> 0) & 0xFFF;
            var tfpm = (l.TimeFlags >> 12) & 0xFFF;
            for (int i = 0; i < TimeFlagsAMCheckedListBox.Items.Count; i++)
            {
                TimeFlagsAMCheckedListBox.SetItemCheckState(i, ((tfam & (1u << i)) > 0) ? CheckState.Checked : CheckState.Unchecked);
            }
            for (int i = 0; i < TimeFlagsPMCheckedListBox.Items.Count; i++)
            {
                TimeFlagsPMCheckedListBox.SetItemCheckState(i, ((tfpm & (1u << i)) > 0) ? CheckState.Checked : CheckState.Unchecked);
            }
        }

        private uint GetFlagsFromItemCheck(CheckedListBox clb, ItemCheckEventArgs e)
        {
            uint flags = 0;
            for (int i = 0; i < clb.Items.Count; i++)
            {
                if ((e != null) && (e.Index == i))
                {
                    if (e.NewValue == CheckState.Checked)
                    {
                        flags += (uint)(1 << i);
                    }
                }
                else
                {
                    if (clb.GetItemChecked(i))
                    {
                        flags += (uint)(1 << i);
                    }
                }
            }
            return flags;
        }

        private void UpdateColour()
        {
            if (populatingui) return;

            var r = (byte)ColourRUpDown.Value;
            var g = (byte)ColourGUpDown.Value;
            var b = (byte)ColourBUpDown.Value;

            ColourLabel.BackColor = System.Drawing.Color.FromArgb(r, g, b);

            if (selectedLight != null)
            {
                selectedLight.ColorR = r;
                selectedLight.ColorG = g;
                selectedLight.ColorB = b;
                UpdateLightParams();
            }
        }

        private void UpdateVolumeColour()
        {
            if (populatingui) return;

            var r = (byte)VolumeColorRUpDown.Value;
            var g = (byte)VolumeColorGUpDown.Value;
            var b = (byte)VolumeColorBUpDown.Value;

            VolumeColorLabel.BackColor = System.Drawing.Color.FromArgb(r, g, b);

            if (selectedLight != null)
            {
                selectedLight.VolumeOuterColorR = r;
                selectedLight.VolumeOuterColorG = g;
                selectedLight.VolumeOuterColorB = b;
            }
        }



        public void SetWidgetModeUI(WidgetMode mode)
        {
            MoveMenuItem.Checked = (mode == WidgetMode.Position);
            MoveMenuItem.BackColor = (mode == WidgetMode.Position) ? SystemColors.GradientActiveCaption : SystemColors.Control;
            RotateMenuItem.Checked = (mode == WidgetMode.Rotation);
            RotateMenuItem.BackColor = (mode == WidgetMode.Rotation) ? SystemColors.GradientActiveCaption : SystemColors.Control;
            ScaleMenuItem.Checked = (mode == WidgetMode.Scale);
            ScaleMenuItem.BackColor = (mode == WidgetMode.Scale) ? SystemColors.GradientActiveCaption : SystemColors.Control;
        }




        private void LightsTreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            SelectLight(e.Node.Tag as LightAttributes);
        }

        private void ModelLightForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            ModelForm.OnLightFormClosed();
        }

        private void GoToButton_Click(object sender, EventArgs e)
        {
            ModelForm.SetCameraPosition(selectedLight.Position, selectedLight.Falloff * 2f);
        }

        private void PositionTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (selectedLight == null) return;
            Vector3 v = FloatUtil.ParseVector3String(PositionTextBox.Text);
            if (selectedLight.Position != v)
            {
                selectedLight.Position = v;
                UpdateLightParams();
            }
        }

        private void TypeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (selectedLight == null) return;
            string type = TypeComboBox.Text;
            switch (type)
            {
                case "Point":
                    selectedLight.Type = LightType.Point;
                    break;
                case "Capsule":
                    selectedLight.Type = LightType.Capsule;
                    break;
                default:
                    selectedLight.Type = LightType.Spot;
                    break;
            }
            UpdateLightParams();
        }

        private void IntensityTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (selectedLight == null) return;
            float v = FloatUtil.Parse(IntensityTextBox.Text);
            if (selectedLight.Intensity != v)
            {
                selectedLight.Intensity = v;
                UpdateLightParams();
            }
        }

        private void ColourRUpDown_ValueChanged(object sender, EventArgs e)
        {
            UpdateColour();
        }

        private void ColourGUpDown_ValueChanged(object sender, EventArgs e)
        {
            UpdateColour();
        }

        private void ColourBUpDown_ValueChanged(object sender, EventArgs e)
        {
            UpdateColour();
        }

        private void ColourLabel_Click(object sender, EventArgs e)
        {
            var colDiag = new ColorDialog { Color = ColourLabel.BackColor };
            if (colDiag.ShowDialog(this) == DialogResult.OK)
            {
                var c = colDiag.Color;
                ColourRUpDown.Value = c.R;
                ColourGUpDown.Value = c.G;
                ColourBUpDown.Value = c.B;
            }
        }

        private void FalloffTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (selectedLight == null) return;
            float v = FloatUtil.Parse(FalloffTextBox.Text);
            if (selectedLight.Falloff != v)
            {
                selectedLight.Falloff = v;
                UpdateLightParams();
            }
        }

        private void FalloffExponentTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (selectedLight == null) return;
            float v = FloatUtil.Parse(FalloffExponentTextBox.Text);
            if (selectedLight.FalloffExponent != v)
            {
                selectedLight.FalloffExponent = v;
                UpdateLightParams();
            }
        }

        private void InnerAngleTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (selectedLight == null) return;
            float v = FloatUtil.Parse(InnerAngleTextBox.Text);
            if (selectedLight.ConeInnerAngle != v)
            {
                selectedLight.ConeInnerAngle = v;
                UpdateLightParams();
            }
        }

        private void OuterAngleTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (selectedLight == null) return;
            float v = FloatUtil.Parse(OuterAngleTextBox.Text);
            if (selectedLight.ConeOuterAngle != v)
            {
                selectedLight.ConeOuterAngle = v;
                UpdateLightParams();
            }
        }

        private void CoronaIntensityTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (selectedLight == null) return;
            float v = FloatUtil.Parse(CoronaIntensityTextBox.Text);
            if (selectedLight.CoronaIntensity != v)
            {
                selectedLight.CoronaIntensity = v;
                UpdateLightParams();
            }
        }

        private void CoronaSizeTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (selectedLight == null) return;
            float v = FloatUtil.Parse(CoronaSizeTextBox.Text);
            if (selectedLight.CoronaSize != v)
            {
                selectedLight.CoronaSize = v;
                UpdateLightParams();
            }
        }

        private void DirectionTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (selectedLight == null) return;
            Vector3 v = FloatUtil.ParseVector3String(DirectionTextBox.Text);
            if (selectedLight.Direction != v)
            {
                selectedLight.Direction = v;
                UpdateLightParams();
            }
        }

        private void NormalizeDirectionButton_Click(object sender, EventArgs e)
        {
            Vector3 d = Vector3.Normalize(FloatUtil.ParseVector3String(DirectionTextBox.Text));
            DirectionTextBox.Text = FloatUtil.GetVector3String(d);
        }

        private void TangentTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (selectedLight == null) return;
            Vector3 v = FloatUtil.ParseVector3String(TangentTextBox.Text);
            if (selectedLight.Tangent != v)
            {
                selectedLight.Tangent = v;
                UpdateLightParams();
            }
        }

        private void NormalizeTangentButton_Click(object sender, EventArgs e)
        {
            Vector3 t = Vector3.Normalize(FloatUtil.ParseVector3String(TangentTextBox.Text));
            TangentTextBox.Text = FloatUtil.GetVector3String(t);
        }

        private void FlagsTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (selectedLight == null) return;
            uint.TryParse(FlagsTextBox.Text, out uint v);
            if (selectedLight.Flags != v)
            {
                selectedLight.Flags = v;
                UpdateLightParams();
            }

        }

        private void FlashinessUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (selectedLight == null) return;
            var v = (byte)FlashinessUpDown.Value;
            if (selectedLight.Flashiness != v)
            {
                selectedLight.Flashiness = v;
                UpdateLightParams();
            }
        }

        private void BoneIDUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (selectedLight == null) return;
            var v = (ushort)BoneIDUpDown.Value;
            if (selectedLight.BoneId != v)
            {
                selectedLight.BoneId = v;
                UpdateLightParams();
            }
        }

        private void GroupIDUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (selectedLight == null) return;
            var v = (byte)GroupIDUpDown.Value;
            if (selectedLight.GroupId != v)
            {
                selectedLight.GroupId = v;
                UpdateLightParams();
            }
        }

        private void ExtentTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (selectedLight == null) return;
            Vector3 v = FloatUtil.ParseVector3String(ExtentTextBox.Text);
            if (selectedLight.Extent != v)
            {
                selectedLight.Extent = v;
                UpdateLightParams();
            }
        }

        private void ShadowBlurUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (selectedLight == null) return;
            var v = (byte)ShadowBlurUpDown.Value;
            if (selectedLight.ShadowBlur != v)
            {
                selectedLight.ShadowBlur = v;
                UpdateLightParams();
            }
        }

        private void LightFadeDistanceUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (selectedLight == null) return;
            var v = (byte)LightFadeDistanceUpDown.Value;
            if (selectedLight.LightFadeDistance != v)
            {
                selectedLight.LightFadeDistance = v;
                UpdateLightParams();
            }
        }

        private void CoronaZBiasTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (selectedLight == null) return;
            float v = FloatUtil.Parse(CoronaZBiasTextBox.Text);
            if (selectedLight.CoronaZBias != v)
            {
                selectedLight.CoronaZBias = v;
                UpdateLightParams();
            }
        }

        private void VolumeIntensityTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (selectedLight == null) return;
            float v = FloatUtil.Parse(VolumeIntensityTextBox.Text);
            if (selectedLight.VolumeIntensity != v)
            {
                selectedLight.VolumeIntensity = v;
                UpdateLightParams();
            }
        }

        private void VolumeSizeScaleTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (selectedLight == null) return;
            float v = FloatUtil.Parse(VolumeSizeScaleTextBox.Text);
            if (selectedLight.VolumeSizeScale != v)
            {
                selectedLight.VolumeSizeScale = v;
                UpdateLightParams();
            }
        }

        private void VolumeOuterExponentTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (selectedLight == null) return;
            float v = FloatUtil.Parse(VolumeOuterExponentTextBox.Text);
            if (selectedLight.VolumeOuterExponent != v)
            {
                selectedLight.VolumeOuterExponent = v;
                UpdateLightParams();
            }
        }

        private void ShadowFadeDistanceUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (selectedLight == null) return;
            var v = (byte)ShadowFadeDistanceUpDown.Value;
            if (selectedLight.ShadowFadeDistance != v)
            {
                selectedLight.ShadowFadeDistance = v;
                UpdateLightParams();
            }
        }

        private void SpecularFadeDistanceUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (selectedLight == null) return;
            var v = (byte)SpecularFadeDistanceUpDown.Value;
            if (selectedLight.SpecularFadeDistance != v)
            {
                selectedLight.SpecularFadeDistance = v;
                UpdateLightParams();
            }
        }

        private void VolumetricFadeDistanceUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (selectedLight == null) return;
            var v = (byte)VolumetricFadeDistanceUpDown.Value;
            if (selectedLight.VolumetricFadeDistance != v)
            {
                selectedLight.VolumetricFadeDistance = v;
                UpdateLightParams();
            }
        }

        private void ShadowNearClipTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (selectedLight == null) return;
            float v = FloatUtil.Parse(ShadowNearClipTextBox.Text);
            if (selectedLight.ShadowNearClip != v)
            {
                selectedLight.ShadowNearClip = v;
                UpdateLightParams();
            }
        }

        private void VolumeColorLabel_Click(object sender, EventArgs e)
        {
            var colDiag = new ColorDialog { Color = ColourLabel.BackColor };
            if (colDiag.ShowDialog(this) == DialogResult.OK)
            {
                var c = colDiag.Color;
                VolumeColorRUpDown.Value = c.R;
                VolumeColorGUpDown.Value = c.G;
                VolumeColorBUpDown.Value = c.B;
            }
        }

        private void VolumeColorRUpDown_ValueChanged(object sender, EventArgs e)
        {
            UpdateVolumeColour();
        }

        private void VolumeColorGUpDown_ValueChanged(object sender, EventArgs e)
        {
            UpdateVolumeColour();
        }

        private void VolumeColorBUpDown_ValueChanged(object sender, EventArgs e)
        {
            UpdateVolumeColour();
        }

        private void TimeFlagsTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (selectedLight == null) return;
            uint.TryParse(TimeFlagsTextBox.Text, out uint v);
            if (selectedLight.TimeFlags != v)
            {
                selectedLight.TimeFlags = v;
            }
            populatingui = true;
            UpdateFlagsCheckBoxes();
            populatingui = false;
        }

        private void TimeFlagsAMCheckedListBox_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (populatingui) return;
            if (selectedLight == null) return;
            var tfam = GetFlagsFromItemCheck(TimeFlagsAMCheckedListBox, e);
            var tfpm = GetFlagsFromItemCheck(TimeFlagsPMCheckedListBox, null);
            var v = tfam + (tfpm << 12);
            if (selectedLight.TimeFlags != v)
            {
                selectedLight.TimeFlags = v;
            }
            populatingui = true;
            TimeFlagsTextBox.Text = selectedLight.TimeFlags.ToString();
            populatingui = false;
        }

        private void TimeFlagsPMCheckedListBox_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (populatingui) return;
            if (selectedLight == null) return;
            var tfam = GetFlagsFromItemCheck(TimeFlagsAMCheckedListBox, null);
            var tfpm = GetFlagsFromItemCheck(TimeFlagsPMCheckedListBox, e);
            var v = tfam + (tfpm << 12);
            if (selectedLight.TimeFlags != v)
            {
                selectedLight.TimeFlags = v;
            }
            populatingui = true;
            TimeFlagsTextBox.Text = selectedLight.TimeFlags.ToString();
            populatingui = false;
        }

        private void TextureHashTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (selectedLight == null) return;
            if (!uint.TryParse(TextureHashTextBox.Text, out uint v))
            {
                v = JenkHash.GenHash(TextureHashTextBox.Text);
            }
            if (selectedLight.ProjectedTextureHash != v)
            {
                selectedLight.ProjectedTextureHash = v;
            }
        }

        private void CullingPlaneNormalTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (selectedLight == null) return;
            Vector3 v = FloatUtil.ParseVector3String(CullingPlaneNormalTextBox.Text);
            if (selectedLight.CullingPlaneNormal != v)
            {
                selectedLight.CullingPlaneNormal = v;
            }
        }

        private void CullingPlaneOffsetTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (selectedLight == null) return;
            float v = FloatUtil.Parse(CullingPlaneOffsetTextBox.Text);
            if (selectedLight.CullingPlaneOffset != v)
            {
                selectedLight.CullingPlaneOffset = v;
                UpdateLightParams();
            }
        }

        private void EditNewLightMenu_Click(object sender, EventArgs e)
        {
            CreateLight();
        }
        
        private void EditDeleteLightMenu_Click(object sender, EventArgs e)
        {
            DeleteLight();
        }

        private void EditDuplicateLightMenu_Click(object sender, EventArgs e)
        {
            DuplicateLight();
        }

        private void OptionsShowOutlinesMenu_Click(object sender, EventArgs e)
        {
            OptionsShowOutlinesMenu.Checked = !OptionsShowOutlinesMenu.Checked;
            ModelForm.showLightGizmos = OptionsShowOutlinesMenu.Checked;
        }

        private void MoveMenuItem_Click(object sender, EventArgs e)
        {
            var mode = MoveMenuItem.Checked ? WidgetMode.Default : WidgetMode.Position;
            SetWidgetModeUI(mode);
            ModelForm.SetWidgetMode(mode);
        }

        private void RotateMenuItem_Click(object sender, EventArgs e)
        {
            var mode = RotateMenuItem.Checked ? WidgetMode.Default : WidgetMode.Rotation;
            SetWidgetModeUI(mode);
            ModelForm.SetWidgetMode(mode);
        }

        private void ScaleMenuItem_Click(object sender, EventArgs e)
        {
            var mode = ScaleMenuItem.Checked ? WidgetMode.Default : WidgetMode.Scale;
            SetWidgetModeUI(mode);
            ModelForm.SetWidgetMode(mode);
        }

        private void NewLightButton_Click(object sender, EventArgs e)
        {
            CreateLight();
        }

        private void DeleteLightButton_Click(object sender, EventArgs e)
        {
            DeleteLight();
        }

        private void DuplicateLightButton_Click(object sender, EventArgs e)
        {
            DuplicateLight();
        }

        private void MainSplitContainer_Panel2_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
