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

        private void EditYtypArchetypePanel_Load(object sender, EventArgs e)
        {
            AssetTypeComboBox.Items.AddRange(Enum.GetNames(typeof(Unk_1991964615)));
        }

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

                EntitySetsListBox.Items.Clear();
                if (CurrentArchetype is MloArchetype MloArchetype)
                {
                    if (!TabControl.TabPages.Contains(MloArchetypeTabPage))
                    {
                        TabControl.TabPages.Add(MloArchetypeTabPage);
                    }

                    MloInstanceData mloinstance = ProjectForm.TryGetMloInstance(MloArchetype);
                    if (mloinstance != null)
                    {
                        EntitySetsListBox.Enabled = true;
                        foreach (var sets in mloinstance.EntitySets)
                        {
                            MloInstanceEntitySet set = sets.Value;
                            EntitySetsListBox.Items.Add(set.EntitySet.ToString(), set.Visible);
                        }
                    }
                    else EntitySetsListBox.Enabled = false;
                }
                else TabControl.TabPages.Remove(MloArchetypeTabPage);
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
                    ProjectForm.SetYtypHasChanged(true);
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
                    ProjectForm.SetYtypHasChanged(true);
                }
            }
        }

        private void TextureDictTextBox_TextChanged(object sender, EventArgs e)
        {
            // Check that the form is not null before locking...
            if (ProjectForm == null)
                return;

            lock (ProjectForm.ProjectSyncRoot)
            {
                // Embedded...
                if (TextureDictTextBox.Text == ArchetypeNameTextBox.Text)
                {
                    TextureDictHashLabel.Text = "Embedded";
                    CurrentArchetype._BaseArchetypeDef.textureDictionary = CurrentArchetype._BaseArchetypeDef.name;
                    return;
                }

                var hash = JenkHash.GenHash(TextureDictTextBox.Text);

                if (CurrentArchetype._BaseArchetypeDef.textureDictionary != hash)
                {
                    var ytd = ProjectForm.GameFileCache.GetYtd(hash);
                    if (ytd == null)
                    {
                        TextureDictHashLabel.Text = "Hash: " + hash.ToString() + " (invalid)";
                        ProjectForm.SetYtypHasChanged(true);
                        return;
                    }
                    TextureDictHashLabel.Text = "Hash: " + hash.ToString();
                    CurrentArchetype._BaseArchetypeDef.textureDictionary = hash;
                    ProjectForm.SetYtypHasChanged(true);
                }
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

                if (CurrentArchetype._BaseArchetypeDef.physicsDictionary != hash)
                {
                    var ytd = ProjectForm.GameFileCache.GetYbn(hash);
                    if (ytd == null)
                    {
                        PhysicsDictHashLabel.Text = "Hash: " + hash.ToString() + " (invalid)";
                        ProjectForm.SetYtypHasChanged(true);
                        return;
                    }
                    PhysicsDictHashLabel.Text = "Hash: " + hash.ToString();

                    CurrentArchetype._BaseArchetypeDef.physicsDictionary = hash;
                    ProjectForm.SetYtypHasChanged(true);
                }
            }
        }

        private void ArchetypeNameTextBox_TextChanged(object sender, EventArgs e)
        {
            var hash = JenkHash.GenHash(ArchetypeNameTextBox.Text);

            if (CurrentArchetype._BaseArchetypeDef.name != hash)
            {
                CurrentArchetype._BaseArchetypeDef.name = hash;
                UpdateFormTitle();

                TreeNode tn = ProjectForm.ProjectExplorer?.FindArchetypeTreeNode(CurrentArchetype);
                if (tn != null)
                    tn.Text = ArchetypeNameTextBox.Text ?? "0"; // using the text box text because the name may not be in the gfc.

                ProjectForm.SetYtypHasChanged(true);
            }
        }

        private void AssetNameTextBox_TextChanged(object sender, EventArgs e)
        {
            var hash = JenkHash.GenHash(AssetNameTextBox.Text);

            if (CurrentArchetype._BaseArchetypeDef.assetName != hash)
            {
                CurrentArchetype._BaseArchetypeDef.assetName = hash;
                ProjectForm.SetYtypHasChanged(true);
            }
        }

        private void ClipDictionaryTextBox_TextChanged(object sender, EventArgs e)
        {
            var hash = JenkHash.GenHash(ClipDictionaryTextBox.Text);

            if (CurrentArchetype._BaseArchetypeDef.clipDictionary != hash)
            {
                CurrentArchetype._BaseArchetypeDef.clipDictionary = hash;
                ProjectForm.SetYtypHasChanged(true);
            }
        }

        private void LodDistNumericUpDown_ValueChanged(object sender, EventArgs e)
        {
            var loddist = (float)LodDistNumericUpDown.Value;
            if (!MathUtil.NearEqual(loddist, CurrentArchetype._BaseArchetypeDef.lodDist))
            {
                CurrentArchetype._BaseArchetypeDef.lodDist = loddist;
                ProjectForm.SetYtypHasChanged(true);
            }
        }

        private void HDTextureDistNumericUpDown_ValueChanged(object sender, EventArgs e)
        {
            var hddist = (float)HDTextureDistNumericUpDown.Value;
            if (!MathUtil.NearEqual(hddist, CurrentArchetype._BaseArchetypeDef.hdTextureDist))
            {
                CurrentArchetype._BaseArchetypeDef.hdTextureDist = hddist;
                ProjectForm.SetYtypHasChanged(true);
            }
        }

        private void SpecialAttributeNumericUpDown_ValueChanged(object sender, EventArgs e)
        {
            var att = (uint)SpecialAttributeNumericUpDown.Value;
            if (CurrentArchetype._BaseArchetypeDef.specialAttribute != att)
            {
                CurrentArchetype._BaseArchetypeDef.specialAttribute = att;
                ProjectForm.SetYtypHasChanged(true);
            }
        }

        private void BBMinTextBox_TextChanged(object sender, EventArgs e)
        {
            Vector3 min = FloatUtil.ParseVector3String(BBMinTextBox.Text);
            if (CurrentArchetype._BaseArchetypeDef.bbMin != min)
            {
                CurrentArchetype._BaseArchetypeDef.bbMin = min;
                ProjectForm.SetYtypHasChanged(true);
            }
        }

        private void BBMaxTextBox_TextChanged(object sender, EventArgs e)
        {
            Vector3 max = FloatUtil.ParseVector3String(BBMaxTextBox.Text);

            if (CurrentArchetype._BaseArchetypeDef.bbMax != max)
            {
                CurrentArchetype._BaseArchetypeDef.bbMax = max;
                ProjectForm.SetYtypHasChanged(true);
            }
        }

        private void BSCenterTextBox_TextChanged(object sender, EventArgs e)
        {
            Vector3 c = FloatUtil.ParseVector3String(BSCenterTextBox.Text);

            if (CurrentArchetype._BaseArchetypeDef.bsCentre != c)
            {
                CurrentArchetype._BaseArchetypeDef.bsCentre = c;
                ProjectForm.SetYtypHasChanged(true);
            }
        }

        private void BSRadiusTextBox_TextChanged(object sender, EventArgs e)
        {
            if (float.TryParse(BSRadiusTextBox.Text, out float f))
            {
                if (!MathUtil.NearEqual(CurrentArchetype._BaseArchetypeDef.bsRadius, f))
                {
                    CurrentArchetype._BaseArchetypeDef.bsRadius = f;
                    ProjectForm.SetYtypHasChanged(true);
                }
            }
            else
            {
                CurrentArchetype._BaseArchetypeDef.bsRadius = 0f;
                ProjectForm.SetYtypHasChanged(true);
            }
        }

        private void DeleteArchetypeButton_Click(object sender, EventArgs e)
        {
            ProjectForm.SetProjectItem(CurrentArchetype);
            ProjectForm.DeleteArchetype();
        }

        private void EntitySetsListBox_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (CurrentArchetype is MloArchetype MloArchetype)
            {
                var inst = ProjectForm.TryGetMloInstance(MloArchetype);
                if (inst != null)
                {
                    MloInstanceEntitySet mloInstanceEntitySet = inst.EntitySets[MloArchetype.entitySets[e.Index]._Data.name];
                    mloInstanceEntitySet.Visible = e.NewValue == CheckState.Checked;
                    return;
                }
            }
            e.NewValue = CheckState.Unchecked;
        }
    }
}
