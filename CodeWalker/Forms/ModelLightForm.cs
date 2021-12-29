using CodeWalker.GameFiles;
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
                string mprefix = "Light" + (mi + 1).ToString();
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
                populatingui = true;
                PositionTextBox.Text = "";
                DirectionTextBox.Text = "";
                TypeComboBox.SelectedItem = "Point";
                ColourRUpDown.Value = 0;
                ColourGUpDown.Value = 0;
                ColourBUpDown.Value = 0;
                IntensityUpDown.Value = 0;
                FlashinessTextBox.Text = "";
                BoneIDTextBox.Text = "";
                GroupIDTextBox.Text = "";
                FalloffTextBox.Text = "";
                FalloffExponentTextBox.Text = "";
                InnerAngleUpDown.Value = 0;
                OuterAngleUpDown.Value = 0;
                CoronaSizeTextBox.Text = "";
                CoronaIntensityUpDown.Value = 0;
                ExtentTextBox.Text = "";
                ShadowBlurTextBox.Text = "";
                LightFadeDistanceTextBox.Text = "";
                CoronaZBiasTextBox.Text = "";
                HashTextBox.Text = "";
                VolumeIntensityTextBox.Text = "";
                VolumeSizeScaleTextBox.Text = "";
                VolumeColorRUpDown.Value = 0;
                VolumeColorGUpDown.Value = 0;
                VolumeColorBUpDown.Value = 0;
                VolumeOuterExponentTextBox.Text = "";
                ShadowFadeDistanceTextBox.Text = "";
                SpecularFadeDistanceTextBox.Text = "";
                VolumetricFadeDistanceTextBox.Text = "";
                ShadowNearClipTextBox.Text = "";
                CullingPlaneNormalTextBox.Text = "";
                CullingPlaneOffsetTextBox.Text = "";
                TimeFlagsTextBox.Text = "";
                populatingui = false;
            }
            else
            {
                populatingui = true;
                PositionTextBox.Text = FloatUtil.GetVector3String(light.Position);
                DirectionTextBox.Text = FloatUtil.GetVector3String(light.Direction);
                TypeComboBox.SelectedItem = light.Type.ToString();
                ColourRUpDown.Value = light.ColorR;
                ColourGUpDown.Value = light.ColorG;
                ColourBUpDown.Value = light.ColorB;
                ColourLabel.BackColor = System.Drawing.Color.FromArgb(light.ColorR, light.ColorG, light.ColorB);
                IntensityUpDown.Value = (decimal)light.Intensity > IntensityUpDown.Maximum ? IntensityUpDown.Maximum : (decimal)light.Intensity;
                FlashinessTextBox.Text = FloatUtil.ToString(light.Flashiness);
                BoneIDTextBox.Text = FloatUtil.ToString(light.BoneId);
                GroupIDTextBox.Text = FloatUtil.ToString(light.GroupId);
                FalloffTextBox.Text = FloatUtil.ToString(light.Falloff);
                FalloffExponentTextBox.Text = FloatUtil.ToString(light.FalloffExponent);
                InnerAngleUpDown.Value = (decimal)light.ConeInnerAngle > InnerAngleUpDown.Maximum ? InnerAngleUpDown.Maximum : (decimal)light.ConeInnerAngle;
                OuterAngleUpDown.Value = (decimal)light.ConeOuterAngle > OuterAngleUpDown.Maximum ? OuterAngleUpDown.Maximum : (decimal)light.ConeOuterAngle;
                CoronaSizeTextBox.Text = FloatUtil.ToString(light.CoronaSize);
                CoronaIntensityUpDown.Value = (decimal)light.CoronaIntensity > CoronaIntensityUpDown.Maximum ? CoronaIntensityUpDown.Maximum : (decimal)light.CoronaIntensity;
                ExtentTextBox.Text = FloatUtil.GetVector3String(light.Extent);
                ShadowBlurTextBox.Text = FloatUtil.ToString(light.ShadowBlur);
                LightFadeDistanceTextBox.Text = FloatUtil.ToString(light.LightFadeDistance);
                CoronaZBiasTextBox.Text = FloatUtil.ToString(light.CoronaZBias);
                HashTextBox.Text = light.ProjectedTextureHash.Hash.ToString();
                VolumeIntensityTextBox.Text = FloatUtil.ToString(light.VolumeIntensity);
                VolumeSizeScaleTextBox.Text = FloatUtil.ToString(light.VolumeSizeScale);
                VolumeColorRUpDown.Value = light.VolumeOuterColorR;
                VolumeColorGUpDown.Value = light.VolumeOuterColorG;
                VolumeColorBUpDown.Value = light.VolumeOuterColorB;
                VolumeColorLabel.BackColor = System.Drawing.Color.FromArgb(light.VolumeOuterColorR, light.VolumeOuterColorG, light.VolumeOuterColorB);
                VolumeOuterExponentTextBox.Text = FloatUtil.ToString(light.VolumeOuterExponent);
                ShadowFadeDistanceTextBox.Text = FloatUtil.ToString(light.ShadowFadeDistance);
                SpecularFadeDistanceTextBox.Text = FloatUtil.ToString(light.SpecularFadeDistance);
                VolumetricFadeDistanceTextBox.Text = FloatUtil.ToString(light.VolumetricFadeDistance);
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
            selectedLight.HasChanged = true;
        }


        private LightAttributes NewLightAttribute()
        {
            LightAttributes light = new LightAttributes();
            light.Direction = Vector3.BackwardLH;
            light.Tangent = Vector3.Normalize(Vector3.BackwardLH.GetPerpVec());
            light.Intensity = 20;
            light.ConeInnerAngle = 5;
            light.ConeOuterAngle = 35;
            light.Extent = Vector3.One;
            light.Type = LightType.Spot;
            light.Falloff = 10;
            light.ColorR = 255;
            light.ColorG = 255;
            light.ColorB = 255;
            return light;
        }
        private void SelectLight(LightAttributes light)
        {
            if (light == null)
            {
                selectedLight = null;
                ModelForm.selectedLight = null;
                UpdateUI();
                ModelForm.UpdateWidget();
            }
            else
            {
                selectedLight = light;
                ModelForm.selectedLight = light;
                UpdateUI();
                ModelForm.UpdateWidget();
            }
        }
        private void CreateLight()
        {
            if(Drawable != null)
            {
                List<LightAttributes> lights = Drawable.LightAttributes.data_items.ToList();
                lights.Add(NewLightAttribute());
                Drawable.LightAttributes.data_items = lights.ToArray();
                UpdateLightParams();
                LoadModel(Drawable);
            }
            else if(FragDrawable != null)
            {
                List<LightAttributes> lights = FragDrawable.OwnerFragment.LightAttributes.data_items.ToList();
                lights.Add(NewLightAttribute());
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
                        List<LightAttributes> lights = dr.LightAttributes.data_items.ToList();
                        lights.Add(NewLightAttribute());
                        dr.LightAttributes.data_items = lights.ToArray();
                        UpdateLightParams();
                        LoadModels(DrawableDict);
                    }
                }
            }
        }
        private void DeleteLight()
        {
            if (selectedLight == null) return;
            if(Drawable != null)
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

        private void NormalizeDirectionButton_Click(object sender, EventArgs e)
        {
            Vector3 d = Vector3.Normalize(FloatUtil.ParseVector3String(DirectionTextBox.Text));
            DirectionTextBox.Text = FloatUtil.GetVector3String(d);
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

        private void IntensityUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (selectedLight == null) return;
            float v = FloatUtil.Parse(IntensityUpDown.Text);
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

        private void InnerAngleUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (selectedLight == null) return;
            float v = FloatUtil.Parse(InnerAngleUpDown.Text);
            if (selectedLight.ConeInnerAngle != v)
            {
                selectedLight.ConeInnerAngle = v;
                UpdateLightParams();
            }
        }

        private void OuterAngleUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (selectedLight == null) return;
            float v = FloatUtil.Parse(OuterAngleUpDown.Text);
            if (selectedLight.ConeOuterAngle != v)
            {
                selectedLight.ConeOuterAngle = v;
                UpdateLightParams();
            }
        }

        private void CoronaIntensityUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (selectedLight == null) return;
            float v = FloatUtil.Parse(CoronaIntensityUpDown.Text);
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
                selectedLight.Tangent = Vector3.Normalize(selectedLight.Direction.GetPerpVec());
                UpdateLightParams();
            }
        }

        private void FlashinessTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (selectedLight == null) return;
            float v = FloatUtil.Parse(FlashinessTextBox.Text);
            if (selectedLight.Flashiness != v)
            {
                selectedLight.Flashiness = (byte)v;
                UpdateLightParams();
            }
        }

        private void BoneIDTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (selectedLight == null) return;
            float v = FloatUtil.Parse(BoneIDTextBox.Text);
            if (selectedLight.BoneId != v)
            {
                selectedLight.BoneId = (ushort)v;
                UpdateLightParams();
            }
        }

        private void GroupIDTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (selectedLight == null) return;
            float v = FloatUtil.Parse(GroupIDTextBox.Text);
            if (selectedLight.GroupId != v)
            {
                selectedLight.GroupId = (byte)v;
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

        private void ShadowBlurTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (selectedLight == null) return;
            float v = FloatUtil.Parse(ShadowBlurTextBox.Text);
            if (selectedLight.ShadowBlur != v)
            {
                selectedLight.ShadowBlur = (byte)v;
                UpdateLightParams();
            }
        }

        private void LightFadeDistanceTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (selectedLight == null) return;
            float v = FloatUtil.Parse(LightFadeDistanceTextBox.Text);
            if (selectedLight.LightFadeDistance != v)
            {
                selectedLight.LightFadeDistance = (byte)v;
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

        private void ShadowFadeDistanceTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (selectedLight == null) return;
            float v = FloatUtil.Parse(ShadowFadeDistanceTextBox.Text);
            if (selectedLight.ShadowFadeDistance != v)
            {
                selectedLight.ShadowFadeDistance = (byte)v;
                UpdateLightParams();
            }
        }

        private void SpecularFadeDistanceTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (selectedLight == null) return;
            float v = FloatUtil.Parse(SpecularFadeDistanceTextBox.Text);
            if (selectedLight.SpecularFadeDistance != v)
            {
                selectedLight.SpecularFadeDistance = (byte)v;
                UpdateLightParams();
            }
        }

        private void VolumetricFadeDistance_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (selectedLight == null) return;
            float v = FloatUtil.Parse(VolumetricFadeDistanceTextBox.Text);
            if (selectedLight.VolumetricFadeDistance != v)
            {
                selectedLight.VolumetricFadeDistance = (byte)v;
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

        private void HashTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (selectedLight == null) return;
            uint.TryParse(HashTextBox.Text, out uint v);
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

        private void lightToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CreateLight();
        }
        
        private void deleteLightToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DeleteLight();
        }

        private void showGizmosToolStripMenuItem_Click(object sender, EventArgs e)
        {
            showGizmosToolStripMenuItem.Checked = !showGizmosToolStripMenuItem.Checked;
            ModelForm.showLightGizmos = showGizmosToolStripMenuItem.Checked;
        }

        private void moveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            moveToolStripMenuItem.Checked = !moveToolStripMenuItem.Checked;
            if (moveToolStripMenuItem.Checked)
            {
                moveToolStripMenuItem.BackColor = System.Drawing.SystemColors.GradientActiveCaption;
                rotateToolStripMenuItem.Checked = false;
                rotateToolStripMenuItem.BackColor = System.Drawing.SystemColors.Control;
                ModelForm.SetWidgetMode("Position");
            }
            else
            {
                moveToolStripMenuItem.BackColor = System.Drawing.SystemColors.Control;
                ModelForm.SetWidgetMode("Default");
            }
        }

        private void rotateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            rotateToolStripMenuItem.Checked = !rotateToolStripMenuItem.Checked;
            if (rotateToolStripMenuItem.Checked)
            {
                rotateToolStripMenuItem.BackColor = System.Drawing.SystemColors.GradientActiveCaption;
                moveToolStripMenuItem.Checked = false;
                moveToolStripMenuItem.BackColor = System.Drawing.SystemColors.Control;
                ModelForm.SetWidgetMode("Rotation");
            }
            else
            {
                rotateToolStripMenuItem.BackColor = System.Drawing.SystemColors.Control;
                ModelForm.SetWidgetMode("Default");
            }
        }
    }
}
