using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using CodeWalker.GameFiles;
using Vector3 = SharpDX.Vector3;

namespace CodeWalker.Project.Panels
{
    public partial class EditMultiPanel
    {
        private TabControl EntityTabControl;
        private TabPage EntityGeneralTabPage;
        private TabPage EntityFlagsTabPage;
        private TabPage EntityLodTabPage;
        private TextBox EntityArchetypeTextBox;
        private TextBox EntityFlagsTextBox;
        private TextBox EntityGuidTextBox;
        private TextBox EntityScaleXYTextBox;
        private TextBox EntityScaleZTextBox;
        private ComboBox EntityPriorityLevelComboBox;
        private TextBox EntityAOMultiplierTextBox;
        private TextBox EntityArtificialAOTextBox;
        private TextBox EntityTintValueTextBox;
        private CheckedListBox EntityFlagsCheckedListBox;
        private TextBox EntityParentIndexTextBox;
        private TextBox EntityLodDistTextBox;
        private TextBox EntityChildLodDistTextBox;
        private ComboBox EntityLodLevelComboBox;
        private TextBox EntityNumChildrenTextBox;
        private YmapEntityDef[] EntityItems;

        private void InitializeEntityBatchControls()
        {
            EntityTabControl = new TabControl();
            EntityGeneralTabPage = new TabPage();
            EntityFlagsTabPage = new TabPage();
            EntityLodTabPage = new TabPage();

            EntityTabControl.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            EntityTabControl.Location = new Point(283, 108);
            EntityTabControl.Name = "EntityTabControl";
            EntityTabControl.Size = new Size(270, 333);
            EntityTabControl.TabIndex = 22;
            EntityTabControl.Visible = false;

            EntityGeneralTabPage.Text = "General";
            EntityGeneralTabPage.UseVisualStyleBackColor = true;
            EntityFlagsTabPage.Text = "Flags";
            EntityFlagsTabPage.AutoScroll = true;
            EntityFlagsTabPage.UseVisualStyleBackColor = true;
            EntityLodTabPage.Text = "LOD Hierarchy";
            EntityLodTabPage.UseVisualStyleBackColor = true;

            EntityTabControl.TabPages.Add(EntityGeneralTabPage);
            EntityTabControl.TabPages.Add(EntityFlagsTabPage);
            EntityTabControl.TabPages.Add(EntityLodTabPage);

            EntityArchetypeTextBox = AddTextBox(EntityGeneralTabPage, "Archetype:", 7, EntityArchetypeTextBox_TextChanged);
            EntityGuidTextBox = AddTextBox(EntityGeneralTabPage, "GUID:", 33, EntityGuidTextBox_TextChanged);
            EntityScaleXYTextBox = AddTextBox(EntityGeneralTabPage, "ScaleXY:", 59, EntityScaleXYTextBox_TextChanged);
            EntityScaleZTextBox = AddTextBox(EntityGeneralTabPage, "ScaleZ:", 85, EntityScaleZTextBox_TextChanged);
            EntityPriorityLevelComboBox = AddComboBox(EntityGeneralTabPage, "PriorityLevel:", 111, EntityPriorityLevelComboBox_SelectedIndexChanged);
            EntityAOMultiplierTextBox = AddTextBox(EntityGeneralTabPage, "AOMultiplier:", 138, EntityAOMultiplierTextBox_TextChanged);
            EntityArtificialAOTextBox = AddTextBox(EntityGeneralTabPage, "ArtificialAO:", 164, EntityArtificialAOTextBox_TextChanged);
            EntityTintValueTextBox = AddTextBox(EntityGeneralTabPage, "TintValue:", 190, EntityTintValueTextBox_TextChanged);

            EntityFlagsTextBox = AddTextBox(EntityFlagsTabPage, "Flags:", 8, EntityFlagsTextBox_TextChanged);
            EntityFlagsCheckedListBox = new CheckedListBox();
            EntityFlagsCheckedListBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            EntityFlagsCheckedListBox.CheckOnClick = true;
            EntityFlagsCheckedListBox.FormattingEnabled = true;
            EntityFlagsCheckedListBox.IntegralHeight = false;
            EntityFlagsCheckedListBox.Location = new Point(6, 34);
            EntityFlagsCheckedListBox.Name = "EntityFlagsCheckedListBox";
            EntityFlagsCheckedListBox.Size = new Size(247, 267);
            EntityFlagsCheckedListBox.TabIndex = 19;
            EntityFlagsCheckedListBox.ItemCheck += EntityFlagsCheckedListBox_ItemCheck;
            EntityFlagsTabPage.Controls.Add(EntityFlagsCheckedListBox);

            EntityParentIndexTextBox = AddTextBox(EntityLodTabPage, "ParentIndex:", 8, EntityParentIndexTextBox_TextChanged, 81);
            EntityLodDistTextBox = AddTextBox(EntityLodTabPage, "LodDist:", 34, EntityLodDistTextBox_TextChanged, 81);
            EntityChildLodDistTextBox = AddTextBox(EntityLodTabPage, "ChildLodDist:", 60, EntityChildLodDistTextBox_TextChanged, 81);
            EntityLodLevelComboBox = AddComboBox(EntityLodTabPage, "LodLevel:", 86, EntityLodLevelComboBox_SelectedIndexChanged, 81);
            EntityNumChildrenTextBox = AddTextBox(EntityLodTabPage, "NumChildren:", 113, EntityNumChildrenTextBox_TextChanged, 81);

            Controls.Add(EntityTabControl);
            LoadEntityBatchDropDowns();
        }

        private TextBox AddTextBox(Control parent, string labelText, int top, EventHandler changed, int inputLeft = 74)
        {
            var label = new Label();
            label.AutoSize = true;
            label.Location = new Point(Math.Max(3, inputLeft - 6 - TextRenderer.MeasureText(labelText, Font).Width), top + 3);
            label.Text = labelText;
            parent.Controls.Add(label);

            var textBox = new TextBox();
            textBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            textBox.Location = new Point(inputLeft, top);
            textBox.Size = new Size(parent.ClientSize.Width - inputLeft - 9, 20);
            textBox.TextChanged += changed;
            parent.Controls.Add(textBox);
            return textBox;
        }

        private ComboBox AddComboBox(Control parent, string labelText, int top, EventHandler changed, int inputLeft = 74)
        {
            var label = new Label();
            label.AutoSize = true;
            label.Location = new Point(Math.Max(3, inputLeft - 6 - TextRenderer.MeasureText(labelText, Font).Width), top + 3);
            label.Text = labelText;
            parent.Controls.Add(label);

            var comboBox = new ComboBox();
            comboBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            comboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox.FormattingEnabled = true;
            comboBox.Location = new Point(inputLeft, top);
            comboBox.Size = new Size(parent.ClientSize.Width - inputLeft - 9, 21);
            comboBox.SelectedIndexChanged += changed;
            parent.Controls.Add(comboBox);
            return comboBox;
        }

        private void LoadEntityBatchDropDowns()
        {
            EntityLodLevelComboBox.Items.Clear();
            EntityLodLevelComboBox.Items.Add(string.Empty);
            EntityLodLevelComboBox.Items.Add(rage__eLodType.LODTYPES_DEPTH_ORPHANHD);
            EntityLodLevelComboBox.Items.Add(rage__eLodType.LODTYPES_DEPTH_HD);
            EntityLodLevelComboBox.Items.Add(rage__eLodType.LODTYPES_DEPTH_LOD);
            EntityLodLevelComboBox.Items.Add(rage__eLodType.LODTYPES_DEPTH_SLOD1);
            EntityLodLevelComboBox.Items.Add(rage__eLodType.LODTYPES_DEPTH_SLOD2);
            EntityLodLevelComboBox.Items.Add(rage__eLodType.LODTYPES_DEPTH_SLOD3);
            EntityLodLevelComboBox.Items.Add(rage__eLodType.LODTYPES_DEPTH_SLOD4);

            EntityPriorityLevelComboBox.Items.Clear();
            EntityPriorityLevelComboBox.Items.Add(string.Empty);
            EntityPriorityLevelComboBox.Items.Add(rage__ePriorityLevel.PRI_REQUIRED);
            EntityPriorityLevelComboBox.Items.Add(rage__ePriorityLevel.PRI_OPTIONAL_HIGH);
            EntityPriorityLevelComboBox.Items.Add(rage__ePriorityLevel.PRI_OPTIONAL_MEDIUM);
            EntityPriorityLevelComboBox.Items.Add(rage__ePriorityLevel.PRI_OPTIONAL_LOW);

            EntityFlagsCheckedListBox.Items.Clear();
            EntityFlagsCheckedListBox.Items.AddRange(new object[] {
                "1 - Allow Full Rotation",
                "2 - Stream Low Priority",
                "4 - Disable embedded Collisions",
                "8  - LOD in parent map",
                "16 - LOD Adopt me",
                "32 - Static Entity",
                "64 - Interior LOD",
                "128 - Unused",
                "256 - Unused",
                "512 - Unused",
                "1024 - Unused",
                "2048 - Unused",
                "4096 - Unused",
                "8192 - Unused ",
                "16384 - Unused",
                "32768 - LOD Use Alt Fade",
                "65536 - Underwater",
                "131072 - Doesn't touch water",
                "262144 - Doesn't spawn peds",
                "524288 - Cast Static Shadows",
                "1048576 - Cast Dynamic Shadows",
                "2097152 - Ignore Time Settings",
                "4194304 - Don't render shadows",
                "8388608 - Only render shadows",
                "16777216 - Don't render reflections",
                "33554432 - Only render reflections",
                "67108864 - Don't render water reflections",
                "134217728 - Only render water reflections",
                "268435456 - Don't render mirror reflections",
                "536870912 - Only render mirror reflections",
                "1073741824 - Unused",
                "2147483648 - Unused" });
        }

        private void SetEntityBatchItems(MapSelection[] items)
        {
            EntityItems = GetEntityItems(items);
            LoadEntityItems();
        }

        private void ClearEntityBatchItems()
        {
            EntityItems = null;
            LoadEntityItems();
        }

        private YmapEntityDef[] GetEntityItems(MapSelection[] items)
        {
            if ((items == null) || (items.Length == 0)) return null;
            var entities = new List<YmapEntityDef>();
            foreach (var item in items)
            {
                var ent = item.EntityDef ?? item.MloEntityDef;
                if (ent == null) return null;
                entities.Add(ent);
            }
            return entities.ToArray();
        }

        private void LoadEntityItems()
        {
            var hasEntities = (EntityItems != null) && (EntityItems.Length > 0);
            EntityTabControl.Visible = hasEntities;
            if (!hasEntities)
            {
                EntityArchetypeTextBox.Text = string.Empty;
                EntityFlagsTextBox.Text = string.Empty;
                EntityGuidTextBox.Text = string.Empty;
                EntityScaleXYTextBox.Text = string.Empty;
                EntityScaleZTextBox.Text = string.Empty;
                EntityLodDistTextBox.Text = string.Empty;
                EntityChildLodDistTextBox.Text = string.Empty;
                EntityParentIndexTextBox.Text = string.Empty;
                EntityNumChildrenTextBox.Text = string.Empty;
                EntityAOMultiplierTextBox.Text = string.Empty;
                EntityArtificialAOTextBox.Text = string.Empty;
                EntityTintValueTextBox.Text = string.Empty;
                EntityLodLevelComboBox.SelectedIndex = 0;
                EntityPriorityLevelComboBox.SelectedIndex = 0;
                for (int i = 0; i < EntityFlagsCheckedListBox.Items.Count; i++)
                {
                    EntityFlagsCheckedListBox.SetItemCheckState(i, CheckState.Unchecked);
                }
                return;
            }

            EntityArchetypeTextBox.Text = GetSharedText(e => e._CEntityDef.archetypeName.ToString());
            EntityFlagsTextBox.Text = GetSharedText(e => e._CEntityDef.flags.ToString());
            EntityGuidTextBox.Text = GetSharedText(e => e._CEntityDef.guid.ToString());
            EntityScaleXYTextBox.Text = GetSharedText(e => FloatUtil.ToString(e._CEntityDef.scaleXY));
            EntityScaleZTextBox.Text = GetSharedText(e => FloatUtil.ToString(e._CEntityDef.scaleZ));
            EntityParentIndexTextBox.Text = GetSharedText(e => e._CEntityDef.parentIndex.ToString());
            EntityLodDistTextBox.Text = GetSharedText(e => FloatUtil.ToString(e._CEntityDef.lodDist));
            EntityChildLodDistTextBox.Text = GetSharedText(e => FloatUtil.ToString(e._CEntityDef.childLodDist));
            EntityNumChildrenTextBox.Text = GetSharedText(e => e._CEntityDef.numChildren.ToString());
            EntityAOMultiplierTextBox.Text = GetSharedText(e => e._CEntityDef.ambientOcclusionMultiplier.ToString());
            EntityArtificialAOTextBox.Text = GetSharedText(e => e._CEntityDef.artificialAmbientOcclusion.ToString());
            EntityTintValueTextBox.Text = GetSharedText(e => e._CEntityDef.tintValue.ToString());
            SelectSharedComboValue(EntityLodLevelComboBox, GetSharedValue(e => e._CEntityDef.lodLevel));
            SelectSharedComboValue(EntityPriorityLevelComboBox, GetSharedValue(e => e._CEntityDef.priorityLevel));
            LoadEntityFlagsCheckedList();
        }

        private string GetSharedText(Func<YmapEntityDef, string> getValue)
        {
            if ((EntityItems == null) || (EntityItems.Length == 0)) return string.Empty;
            var first = getValue(EntityItems[0]);
            for (int i = 1; i < EntityItems.Length; i++)
            {
                if (getValue(EntityItems[i]) != first) return string.Empty;
            }
            return first;
        }

        private T? GetSharedValue<T>(Func<YmapEntityDef, T> getValue) where T : struct
        {
            if ((EntityItems == null) || (EntityItems.Length == 0)) return null;
            var first = getValue(EntityItems[0]);
            for (int i = 1; i < EntityItems.Length; i++)
            {
                if (!EqualityComparer<T>.Default.Equals(getValue(EntityItems[i]), first)) return null;
            }
            return first;
        }

        private void SelectSharedComboValue<T>(ComboBox comboBox, T? value) where T : struct
        {
            comboBox.SelectedItem = value.HasValue ? (object)value.Value : string.Empty;
        }

        private void LoadEntityFlagsCheckedList()
        {
            if ((EntityItems == null) || (EntityItems.Length == 0)) return;
            for (int i = 0; i < EntityFlagsCheckedListBox.Items.Count; i++)
            {
                var mask = (uint)(1 << i);
                var first = (EntityItems[0]._CEntityDef.flags & mask) != 0;
                var mixed = false;
                for (int j = 1; j < EntityItems.Length; j++)
                {
                    if (((EntityItems[j]._CEntityDef.flags & mask) != 0) != first)
                    {
                        mixed = true;
                        break;
                    }
                }
                EntityFlagsCheckedListBox.SetItemCheckState(i, mixed ? CheckState.Indeterminate : (first ? CheckState.Checked : CheckState.Unchecked));
            }
        }

        private MCEntityDef GetMCEntity(YmapEntityDef entity)
        {
            return entity?.MloParent?.MloInstance?.TryGetArchetypeEntity(entity);
        }

        private void MarkEntityChanged(YmapEntityDef entity)
        {
            if (entity?.Ymap != null)
            {
                entity.Ymap.HasChanged = true;
                ProjectForm.ProjectExplorer?.SetYmapHasChanged(entity.Ymap, true);
            }
            else if (entity?.MloParent?.Archetype?.Ytyp != null)
            {
                entity.MloParent.Archetype.Ytyp.HasChanged = true;
                ProjectForm.ProjectExplorer?.SetYtypHasChanged(entity.MloParent.Archetype.Ytyp, true);
            }
        }

        private void ApplyToEntities(Action<YmapEntityDef, MCEntityDef> apply)
        {
            if ((EntityItems == null) || (EntityItems.Length == 0)) return;
            lock (ProjectForm.ProjectSyncRoot)
            {
                foreach (var entity in EntityItems)
                {
                    apply(entity, GetMCEntity(entity));
                    MarkEntityChanged(entity);
                    ProjectForm.ProjectExplorer?.UpdateEntityTreeNode(entity);
                }
            }
        }

        private void RefreshEntityGraphics()
        {
            var wf = ProjectForm.WorldForm;
            wf?.BeginInvoke(new Action(() =>
            {
                if (Items != null)
                {
                    MultiItem.SetMultipleSelectionItems(Items);
                    MultiItem.UpdateGraphics(wf);
                }
            }));
        }

        private void EntityArchetypeTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (string.IsNullOrWhiteSpace(EntityArchetypeTextBox.Text)) return;
            uint hash = 0;
            string name = EntityArchetypeTextBox.Text;
            if (!uint.TryParse(name, out hash))
            {
                hash = JenkHash.GenHash(name);
                JenkIndex.Ensure(name);
            }
            var arch = ProjectForm.GameFileCache.GetArchetype(hash);
            ApplyToEntities((entity, mcEntity) =>
            {
                entity._CEntityDef.archetypeName = new MetaHash(hash);
                if (mcEntity != null) mcEntity._Data.archetypeName = new MetaHash(hash);
                if (entity.Archetype != arch)
                {
                    entity.SetArchetype(arch);
                    if (entity.IsMlo)
                    {
                        entity.MloInstance.InitYmapEntityArchetypes(ProjectForm.GameFileCache);
                    }
                }
            });
            RefreshEntityGraphics();
        }

        private void EntityFlagsTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (!uint.TryParse(EntityFlagsTextBox.Text, out uint flags)) return;
            populatingui = true;
            for (int i = 0; i < EntityFlagsCheckedListBox.Items.Count; i++)
            {
                var c = ((flags & (1u << i)) > 0);
                EntityFlagsCheckedListBox.SetItemCheckState(i, c ? CheckState.Checked : CheckState.Unchecked);
            }
            populatingui = false;
            ApplyToEntities((entity, mcEntity) =>
            {
                entity._CEntityDef.flags = flags;
                if (mcEntity != null) mcEntity._Data.flags = flags;
            });
        }

        private void EntityFlagsCheckedListBox_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (populatingui) return;
            var mask = (uint)(1 << e.Index);
            var set = e.NewValue == CheckState.Checked;
            ApplyToEntities((entity, mcEntity) =>
            {
                var flags = entity._CEntityDef.flags;
                flags = set ? (flags | mask) : (flags & ~mask);
                entity._CEntityDef.flags = flags;
                if (mcEntity != null) mcEntity._Data.flags = flags;
            });
            BeginInvoke(new Action(() =>
            {
                if (EntityItems == null) return;
                populatingui = true;
                EntityFlagsTextBox.Text = GetSharedText(ent => ent._CEntityDef.flags.ToString());
                LoadEntityFlagsCheckedList();
                populatingui = false;
            }));
        }

        private void EntityGuidTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (!uint.TryParse(EntityGuidTextBox.Text, out uint guid)) return;
            ApplyToEntities((entity, mcEntity) =>
            {
                entity._CEntityDef.guid = guid;
                if (mcEntity != null) mcEntity._Data.guid = guid;
            });
        }

        private void EntityScaleXYTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (!FloatUtil.TryParse(EntityScaleXYTextBox.Text, out float scaleXY)) return;
            ApplyToEntities((entity, mcEntity) => entity.SetScale(new Vector3(scaleXY, scaleXY, entity.Scale.Z)));
            RefreshEntityGraphics();
        }

        private void EntityScaleZTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (!FloatUtil.TryParse(EntityScaleZTextBox.Text, out float scaleZ)) return;
            ApplyToEntities((entity, mcEntity) => entity.SetScale(new Vector3(entity.Scale.X, entity.Scale.Y, scaleZ)));
            RefreshEntityGraphics();
        }

        private void EntityParentIndexTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (!int.TryParse(EntityParentIndexTextBox.Text, out int parentIndex)) return;
            ApplyToEntities((entity, mcEntity) =>
            {
                entity._CEntityDef.parentIndex = parentIndex;
                if (mcEntity != null) mcEntity._Data.parentIndex = parentIndex;
            });
        }

        private void EntityLodDistTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (!FloatUtil.TryParse(EntityLodDistTextBox.Text, out float lodDist)) return;
            ApplyToEntities((entity, mcEntity) =>
            {
                entity._CEntityDef.lodDist = lodDist;
                if (mcEntity != null) mcEntity._Data.lodDist = lodDist;
            });
        }

        private void EntityChildLodDistTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (!FloatUtil.TryParse(EntityChildLodDistTextBox.Text, out float childLodDist)) return;
            ApplyToEntities((entity, mcEntity) =>
            {
                entity._CEntityDef.childLodDist = childLodDist;
                if (mcEntity != null) mcEntity._Data.childLodDist = childLodDist;
            });
        }

        private void EntityLodLevelComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (!(EntityLodLevelComboBox.SelectedItem is rage__eLodType lodLevel)) return;
            ApplyToEntities((entity, mcEntity) =>
            {
                entity._CEntityDef.lodLevel = lodLevel;
                if (mcEntity != null) mcEntity._Data.lodLevel = lodLevel;
            });
        }

        private void EntityNumChildrenTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (!uint.TryParse(EntityNumChildrenTextBox.Text, out uint numChildren)) return;
            ApplyToEntities((entity, mcEntity) =>
            {
                entity._CEntityDef.numChildren = numChildren;
                if (mcEntity != null) mcEntity._Data.numChildren = numChildren;
            });
        }

        private void EntityPriorityLevelComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (!(EntityPriorityLevelComboBox.SelectedItem is rage__ePriorityLevel priorityLevel)) return;
            ApplyToEntities((entity, mcEntity) =>
            {
                entity._CEntityDef.priorityLevel = priorityLevel;
                if (mcEntity != null) mcEntity._Data.priorityLevel = priorityLevel;
            });
        }

        private void EntityAOMultiplierTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (!int.TryParse(EntityAOMultiplierTextBox.Text, out int aoMultiplier)) return;
            ApplyToEntities((entity, mcEntity) =>
            {
                entity._CEntityDef.ambientOcclusionMultiplier = aoMultiplier;
                if (mcEntity != null) mcEntity._Data.ambientOcclusionMultiplier = aoMultiplier;
            });
        }

        private void EntityArtificialAOTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (!int.TryParse(EntityArtificialAOTextBox.Text, out int artificialAO)) return;
            ApplyToEntities((entity, mcEntity) =>
            {
                entity._CEntityDef.artificialAmbientOcclusion = artificialAO;
                if (mcEntity != null) mcEntity._Data.artificialAmbientOcclusion = artificialAO;
            });
        }

        private void EntityTintValueTextBox_TextChanged(object sender, EventArgs e)
        {
            if (populatingui) return;
            if (!uint.TryParse(EntityTintValueTextBox.Text, out uint tintValue)) return;
            ApplyToEntities((entity, mcEntity) =>
            {
                entity._CEntityDef.tintValue = tintValue;
                if (mcEntity != null) mcEntity._Data.tintValue = tintValue;
            });
        }
    }
}
