using System;
using System.Globalization;
using System.Windows.Forms;
using CodeWalker.GameFiles;
using SharpDX;

namespace CodeWalker.Project.Panels
{
    public partial class EditYtypArchetypePanel : ProjectPanel
    {
        public ProjectForm ProjectForm;

        private bool populatingui;

        public EditYtypArchetypePanel(ProjectForm owner)
        {
            InitializeComponent();

            ProjectForm = owner;
        }

        public Archetype CurrentArchetype { get; set; }

        public void SetArchetype(Archetype archetype)
        {
            CurrentArchetype = archetype;
            Tag = archetype;
            UpdateFormTitle();
            UpdateControls();
        }

        private void UpdateFormTitle()
        {
            Text = CurrentArchetype?.Name ?? "Edit Archetype";
        }

        private void UpdateControls()
        {
            if (CurrentArchetype != null)
            {
                ArchetypeDeleteButton.Enabled = ProjectForm.YtypExistsInProject(CurrentArchetype.Ytyp);
                ArchetypeNameTextBox.Text = CurrentArchetype.Name;
                AssetNameTextBox.Text = CurrentArchetype.AssetName;
                LodDistNumericUpDown.Value = (decimal)CurrentArchetype._BaseArchetypeDef.lodDist;
                HDTextureDistNumericUpDown.Value = (decimal)CurrentArchetype._BaseArchetypeDef.hdTextureDist;
                SpecialAttributeNumericUpDown.Value = CurrentArchetype._BaseArchetypeDef.specialAttribute;
                ArchetypeFlagsTextBox.Text = CurrentArchetype._BaseArchetypeDef.flags.ToString();
                TextureDictTextBox.Text = CurrentArchetype._BaseArchetypeDef.textureDictionary.ToCleanString();
                ClipDictionaryTextBox.Text = CurrentArchetype._BaseArchetypeDef.clipDictionary.ToCleanString();
                PhysicsDictionaryTextBox.Text = CurrentArchetype._BaseArchetypeDef.physicsDictionary.ToCleanString();
                AssetTypeComboBox.Text = CurrentArchetype._BaseArchetypeDef.assetType.ToString();
                BBMinTextBox.Text = FloatUtil.GetVector3String(CurrentArchetype._BaseArchetypeDef.bbMin);
                BBMaxTextBox.Text = FloatUtil.GetVector3String(CurrentArchetype._BaseArchetypeDef.bbMax);
                BSCenterTextBox.Text = FloatUtil.GetVector3String(CurrentArchetype._BaseArchetypeDef.bsCentre);
                BSRadiusTextBox.Text = CurrentArchetype._BaseArchetypeDef.bsRadius.ToString(CultureInfo.InvariantCulture);

                if (!(CurrentArchetype is MloArchetype))
                {
                    // Not editable, let's remove the page.
                    var page = TabControl.TabPages["MloDef"];
                    if (page != null) TabControl.TabPages.Remove(page);
                }
            }
        }

        private void ArchetypeFlagsTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (CurrentArchetype == null) return;
            uint flags = 0;
            uint.TryParse(ArchetypeFlagsTextBox.Text, out flags);
            populatingui = true;
            for (int i = 0; i < EntityFlagsCheckedListBox.Items.Count; i++)
            {
                var c = ((flags & (1u << i)) > 0);
                EntityFlagsCheckedListBox.SetItemCheckState(i, c ? CheckState.Checked : CheckState.Unchecked);
            }
            populatingui = false;
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CurrentArchetype._BaseArchetypeDef.flags != flags)
                {
                    CurrentArchetype._BaseArchetypeDef.flags = flags;
                    ProjectForm.SetYmapHasChanged(true);
                }
            }
        }

        private void ArchetypeFlagsCheckedListBox_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (populatingui) return;
            if (CurrentArchetype == null) return;
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
            ArchetypeFlagsTextBox.Text = flags.ToString();
            populatingui = false;
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (CurrentArchetype._BaseArchetypeDef.flags != flags)
                {
                    CurrentArchetype._BaseArchetypeDef.flags = flags;
                    ProjectForm.SetYmapHasChanged(true);
                }
            }
        }

        private void TextureDictTextBox_TextChanged(object sender, EventArgs e)
        {
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (ProjectForm == null)
                {
                    return;
                }

                // Embedded...
                if (TextureDictTextBox.Text == ArchetypeNameTextBox.Text)
                {
                    TextureDictHashLabel.Text = "Embedded";
                    CurrentArchetype._BaseArchetypeDef.textureDictionary = CurrentArchetype._BaseArchetypeDef.name;
                    return;
                }

                var hash = JenkHash.GenHash(TextureDictTextBox.Text);
                var ytd = ProjectForm.GameFileCache.GetYtd(hash);
                if (ytd == null)
                {
                    TextureDictHashLabel.Text = "Hash: " + hash.ToString() + " (invalid)";
                    return;
                }
                TextureDictHashLabel.Text = "Hash: " + hash.ToString();
                CurrentArchetype._BaseArchetypeDef.textureDictionary = hash;
            }
        }

        private void PhysicsDictionaryTextBox_TextChanged(object sender, EventArgs e)
        {
            lock (ProjectForm.ProjectSyncRoot)
            {
                if (ProjectForm == null)
                {
                    return;
                }

                // Embedded...
                if (PhysicsDictionaryTextBox.Text == ArchetypeNameTextBox.Text)
                {
                    PhysicsDictHashLabel.Text = "Embedded";
                    CurrentArchetype._BaseArchetypeDef.physicsDictionary = CurrentArchetype._BaseArchetypeDef.name;
                    return;
                }

                var hash = JenkHash.GenHash(PhysicsDictionaryTextBox.Text);
                var ytd = ProjectForm.GameFileCache.GetYbn(hash);
                if (ytd == null)
                {
                    PhysicsDictHashLabel.Text = "Hash: " + hash.ToString() + " (invalid)";
                    return;
                }
                PhysicsDictHashLabel.Text = "Hash: " + hash.ToString();
                CurrentArchetype._BaseArchetypeDef.physicsDictionary = hash;
            }
        }

        private void ArchetypeNameTextBox_TextChanged(object sender, EventArgs e)
        {
            var hash = JenkHash.GenHash(ArchetypeNameTextBox.Text);
            CurrentArchetype._BaseArchetypeDef.name = hash;
            UpdateFormTitle();
        }

        private void AssetNameTextBox_TextChanged(object sender, EventArgs e)
        {
            var hash = JenkHash.GenHash(AssetNameTextBox.Text);
            CurrentArchetype._BaseArchetypeDef.assetName = hash;
        }

        private void ClipDictionaryTextBox_TextChanged(object sender, EventArgs e)
        {
            var hash = JenkHash.GenHash(ClipDictionaryTextBox.Text);
            CurrentArchetype._BaseArchetypeDef.clipDictionary = hash;
        }

        private void LodDistNumericUpDown_ValueChanged(object sender, EventArgs e)
        {
            CurrentArchetype._BaseArchetypeDef.lodDist = (float)LodDistNumericUpDown.Value;
        }

        private void HDTextureDistNumericUpDown_ValueChanged(object sender, EventArgs e)
        {
            CurrentArchetype._BaseArchetypeDef.hdTextureDist = (float)HDTextureDistNumericUpDown.Value;
        }

        private void SpecialAttributeNumericUpDown_ValueChanged(object sender, EventArgs e)
        {
            CurrentArchetype._BaseArchetypeDef.specialAttribute = (uint)SpecialAttributeNumericUpDown.Value;
        }

        private void EditYtypArchetypePanel_Load(object sender, EventArgs e)
        {
        }

        private void BBMinTextBox_TextChanged(object sender, EventArgs e)
        {
            Vector3 min = FloatUtil.ParseVector3String(BBMinTextBox.Text);
            CurrentArchetype._BaseArchetypeDef.bbMin = min;
        }

        private void BBMaxTextBox_TextChanged(object sender, EventArgs e)
        {
            Vector3 max = FloatUtil.ParseVector3String(BBMaxTextBox.Text);
            CurrentArchetype._BaseArchetypeDef.bbMax = max;
        }

        private void BSCenterTextBox_TextChanged(object sender, EventArgs e)
        {
            Vector3 c = FloatUtil.ParseVector3String(BSCenterTextBox.Text);
            CurrentArchetype._BaseArchetypeDef.bsCentre = c;
        }

        private void BSRadiusTextBox_TextChanged(object sender, EventArgs e)
        {
            if (float.TryParse(BSRadiusTextBox.Text, out float f))
            {
                CurrentArchetype._BaseArchetypeDef.bsRadius = f;
            }
            else
            {
                CurrentArchetype._BaseArchetypeDef.bsRadius = 0f;
            }
        }

        private void DeleteArchetypeButton_Click(object sender, EventArgs e)
        {
            ProjectForm.SetProjectItem(CurrentArchetype);
            ProjectForm.DeleteArchetype();
        }
    }
}
