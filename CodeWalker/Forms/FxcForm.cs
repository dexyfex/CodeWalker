using CodeWalker.GameFiles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace CodeWalker.Forms
{
    public partial class FxcForm : Form
    {
        private FxcFile Fxc;
        private AwcShaderFile AwcShader;
        private AwcShader SelectedAwcShader;
        private RpfFileEntry rpfFileEntry;
        private ExploreForm exploreForm;

        private string fileName;
        public string FileName
        {
            get { return fileName; }
            set
            {
                fileName = value;
                UpdateFormTitle();
            }
        }
        public string FilePath { get; set; }


        public FxcForm()
        {
            InitializeComponent();
            if (TypeFilterComboBox.Items.Count > 0)
                TypeFilterComboBox.SelectedIndex = 0;
            UpdateAwcModeUi(awcMode: false);
            this.Activated += (s, e) => RefreshEditModeUi();
        }

        private bool IsEditable => AwcShader != null && (exploreForm?.EditMode ?? false);

        private void RefreshEditModeUi()
        {
            if (AwcShader == null) return;
            bool editable = IsEditable;
            SaveMenuItem.Enabled = editable && rpfFileEntry != null;
            ImportCsoMenuItem.Enabled = editable && SelectedAwcShader != null;
        }


        private void UpdateFormTitle()
        {
            string suffix = AwcShader != null ? "AWC Shader Library Viewer" : "FXC Viewer";
            Text = fileName + " - " + suffix + " - CodeWalker by dexyfex";
        }

        private void UpdateAwcModeUi(bool awcMode)
        {
            // Menu items only meaningful in AWC mode. Save / Import are further
            // gated on RPF Explorer's edit mode in RefreshEditModeUi() and on
            // selection in ShaderContextMenu_Opening.
            SaveMenuItem.Enabled = false;
            SaveAsMenuItem.Enabled = awcMode;
            ExportAllMenuItem.Enabled = awcMode;

            // Search/type filter only for AWC (FXC list is small and unsegmented).
            // SearchPanel is docked Top; toggling visibility lets the docked
            // ShadersListView fill the freed space automatically.
            SearchPanel.Visible = awcMode;

            // Hide Type column in FXC mode
            ShadersTypeColumn.Width = awcMode ? 40 : 0;

            // Hide Techniques tab in AWC mode (AWC has no techniques)
            if (awcMode)
            {
                if (MainTabControl.TabPages.Contains(TechniquesTabPage))
                    MainTabControl.TabPages.Remove(TechniquesTabPage);
            }
            else
            {
                if (!MainTabControl.TabPages.Contains(TechniquesTabPage))
                    MainTabControl.TabPages.Insert(1, TechniquesTabPage);
            }
        }


        public void LoadFxc(FxcFile fxc)
        {
            Fxc = fxc;
            AwcShader = null;
            UpdateAwcModeUi(awcMode: false);

            fileName = fxc?.Name;
            if (string.IsNullOrEmpty(fileName))
            {
                fileName = fxc?.FileEntry?.Name;
            }

            UpdateFormTitle();

            DetailsPropertyGrid.SelectedObject = fxc;


            ShadersListView.Items.Clear();
            TechniquesListView.Items.Clear();
            if ((fxc == null) || (fxc.Shaders == null)) return;

            foreach (var shader in fxc.Shaders)
            {
                var item = ShadersListView.Items.Add(string.Empty); // Type col empty in FXC mode
                item.SubItems.Add(shader.Name);
                item.Tag = shader;
            }

            if (fxc.Techniques != null)
            {
                foreach (var technique in fxc.Techniques)
                {
                    var item = TechniquesListView.Items.Add(technique.ToString());
                    item.Tag = technique;
                }
            }


            StatusLabel.Text = (fxc.Shaders?.Length ?? 0) + " shaders, " + (fxc.Techniques?.Length ?? 0) + " techniques";
        }


        public void LoadAwcShader(AwcShaderFile awc, RpfFileEntry entry, ExploreForm owner)
        {
            Fxc = null;
            AwcShader = awc;
            rpfFileEntry = entry;
            exploreForm = owner;
            UpdateAwcModeUi(awcMode: true);

            fileName = entry?.Name ?? awc?.Name;
            UpdateFormTitle();

            DetailsPropertyGrid.SelectedObject = awc;

            RebuildShadersList();
            RefreshEditModeUi();

            StatusLabel.Text = BuildAwcStatus();
        }

        private string BuildAwcStatus()
        {
            if (AwcShader == null) return "Ready";
            return AwcShader.TotalShaderCount + " shaders ("
                + AwcShader.VertexCount + " VS, "
                + AwcShader.PixelCount + " PS, "
                + AwcShader.GeometryCount + " GS, "
                + AwcShader.DomainCount + " DS, "
                + AwcShader.HullCount + " HS, "
                + AwcShader.ComputeCount + " CS)";
        }

        private void RebuildShadersList()
        {
            ShadersListView.BeginUpdate();
            try
            {
                ShadersListView.Items.Clear();
                if (AwcShader == null) return;

                string filter = SearchTextBox.Text?.Trim();
                bool hasFilter = !string.IsNullOrEmpty(filter);
                string typeFilter = (TypeFilterComboBox.SelectedItem as string) ?? "All";

                foreach (var s in AwcShader.AllShaders())
                {
                    if (typeFilter != "All" && !MatchesStage(s.Stage, typeFilter)) continue;
                    if (hasFilter && (s.Name == null || s.Name.IndexOf(filter, StringComparison.OrdinalIgnoreCase) < 0)) continue;

                    var item = ShadersListView.Items.Add(s.StageName);
                    item.SubItems.Add(s.Name);
                    item.Tag = s;
                }
            }
            finally
            {
                ShadersListView.EndUpdate();
            }
        }

        private static bool MatchesStage(AwcShaderStage stage, string label)
        {
            switch (label)
            {
                case "Vertex":   return stage == AwcShaderStage.Vertex;
                case "Pixel":    return stage == AwcShaderStage.Pixel;
                case "Geometry": return stage == AwcShaderStage.Geometry;
                case "Domain":   return stage == AwcShaderStage.Domain;
                case "Hull":     return stage == AwcShaderStage.Hull;
                case "Compute":  return stage == AwcShaderStage.Compute;
                default: return true;
            }
        }


        private void LoadShader(FxcShader s)
        {
            if (s == null)
            {
                ShaderPanel.Enabled = false;
                ShaderTextBox.Text = string.Empty;
            }
            else
            {
                ShaderPanel.Enabled = true;
                FxcParser.ParseShader(s);
                if (!string.IsNullOrEmpty(s.LastError))
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append("Error: ");
                    sb.AppendLine(s.LastError);
                    sb.AppendLine();
                    sb.AppendLine(s.Disassembly);
                    ShaderTextBox.Text = sb.ToString();
                }
                else
                {
                    ShaderTextBox.Text = s.Disassembly;
                }
            }
        }

        private void LoadAwcShader(AwcShader s)
        {
            SelectedAwcShader = s;
            DetailsPropertyGrid.SelectedObject = (object)s ?? AwcShader;

            if (s == null)
            {
                ShaderPanel.Enabled = false;
                ShaderTextBox.Text = string.Empty;
                return;
            }
            ShaderPanel.Enabled = true;

            ShaderTextBox.Text = BuildShaderHeader(s);
        }

        private static string BuildShaderHeader(AwcShader s)
        {
            var sb = new StringBuilder();
            sb.AppendLine("// " + s.StageName + " " + s.Name);
            sb.AppendLine("// Hash:    " + s.HashHex);
            sb.AppendLine("// Wave:    " + s.WaveSize);
            sb.AppendLine("// Size:    " + s.Size + " bytes");
            sb.AppendLine("// Block:   " + s.BlockSize + " bytes");
            sb.AppendLine("// Counts:  reg=" + s.RegCount + " cb=" + s.CBufferCount + " tex=" + s.TexCount);

            if (s.Registers != null && s.Registers.Length > 0)
            {
                sb.AppendLine("//");
                sb.AppendLine("// Registers:");
                foreach (var r in s.Registers)
                {
                    sb.Append("//   ").Append(r.Slot.PadRight(10)).Append(' ').Append((r.Name ?? string.Empty).PadRight(32)).Append("  (").Append(r.ResourceType).AppendLine(")");
                    if (r.CBuffers != null && r.CBuffers.Length > 0)
                    {
                        foreach (var cb in r.CBuffers)
                        {
                            sb.Append("//     +0x").Append(cb.PackOffset.ToString("X4")).Append("  ")
                              .Append(cb.Type).Append(cb.ArraySize > 1 ? "[" + cb.ArraySize + "]" : string.Empty)
                              .Append("  ").AppendLine(cb.Name);
                        }
                    }
                }
            }
            return sb.ToString();
        }

        private void LoadTechnique(FxcTechnique t)
        {
            if (t == null)
            {
                TechniquePanel.Enabled = false;
                TechniqueTextBox.Text = string.Empty;
            }
            else
            {
                TechniquePanel.Enabled = true;
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("technique " + t.Name);
                sb.AppendLine("{");
                if (t.Passes != null)
                {
                    for (int i = 0; i < t.Passes.Length; i++)
                    {
                        var pass = t.Passes[i];
                        sb.AppendLine(" pass p" + i.ToString());
                        sb.AppendLine(" {");

                        var vs = Fxc?.GetVS(pass.VS);
                        var ps = Fxc?.GetPS(pass.PS);
                        var cs = Fxc?.GetCS(pass.CS);
                        var ds = Fxc?.GetDS(pass.DS);
                        var gs = Fxc?.GetGS(pass.GS);
                        var hs = Fxc?.GetHS(pass.HS);

                        if (vs != null) sb.AppendLine("  vertexShader = " + vs.Name + "();");
                        if (ps != null) sb.AppendLine("  pixelShader = " + ps.Name + "();");
                        if (cs != null) sb.AppendLine("  computeShader = " + cs.Name + "();");
                        if (ds != null) sb.AppendLine("  domainShader = " + ds.Name + "();");
                        if (gs != null) sb.AppendLine("  geometryShader = " + gs.Name + "();");
                        if (hs != null) sb.AppendLine("  hullShader = " + hs.Name + "();");

                        sb.AppendLine(" }");
                    }
                }
                sb.AppendLine("}");
                TechniqueTextBox.Text = sb.ToString();
            }
        }


        private void ShadersListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ShadersListView.SelectedItems.Count != 1) return;
            var tag = ShadersListView.SelectedItems[0].Tag;
            if (tag is FxcShader fs) LoadShader(fs);
            else if (tag is AwcShader awcs) LoadAwcShader(awcs);
            else { LoadShader(null); LoadAwcShader((AwcShader)null); }
        }

        private void TechniquesListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            FxcTechnique t = null;
            if (TechniquesListView.SelectedItems.Count == 1)
            {
                t = TechniquesListView.SelectedItems[0].Tag as FxcTechnique;
            }
            LoadTechnique(t);
        }

        // ---------- AWC: search / filter ----------

        private void SearchTextBox_TextChanged(object sender, EventArgs e)
        {
            if (AwcShader != null) RebuildShadersList();
        }

        private void TypeFilterComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (AwcShader != null) RebuildShadersList();
        }

        // ---------- AWC: export / import ----------

        private void ShaderContextMenu_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            bool hasSelection = AwcShader != null && SelectedAwcShader != null;
            ExportCsoMenuItem.Enabled = hasSelection;
            ImportCsoMenuItem.Enabled = hasSelection && IsEditable;
            ImportCsoMenuItem.ToolTipText = (hasSelection && !IsEditable)
                ? "Enable Edit Mode in RPF Explorer to import shaders."
                : null;
            if (AwcShader == null) e.Cancel = true; // hide menu entirely in FXC mode
        }

        private void ExportCsoMenuItem_Click(object sender, EventArgs e)
        {
            var s = SelectedAwcShader;
            if (s == null) { MessageBox.Show("Select a shader to export."); return; }
            using (var sfd = new SaveFileDialog())
            {
                sfd.Filter = "Compiled Shader (*.cso)|*.cso|All files (*.*)|*.*";
                sfd.FileName = SafeFileName(s.StageName + "_" + s.Name) + ".cso";
                if (sfd.ShowDialog() != DialogResult.OK) return;
                File.WriteAllBytes(sfd.FileName, s.Binary ?? Array.Empty<byte>());
                StatusLabel.Text = "Exported " + s.Name + " (" + (s.Binary?.Length ?? 0) + " bytes)";
            }
        }

        private void ImportCsoMenuItem_Click(object sender, EventArgs e)
        {
            if (!IsEditable)
            {
                MessageBox.Show("Enable Edit Mode in RPF Explorer to modify AWC files.",
                    "Edit Mode required", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            var s = SelectedAwcShader;
            if (s == null) { MessageBox.Show("Select a shader to replace."); return; }
            using (var ofd = new OpenFileDialog())
            {
                ofd.Filter = "Compiled Shader (*.cso)|*.cso|All files (*.*)|*.*";
                if (ofd.ShowDialog() != DialogResult.OK) return;
                byte[] bytes = File.ReadAllBytes(ofd.FileName);
                if (bytes.Length < 4)
                {
                    MessageBox.Show("File too small to be a CSO.");
                    return;
                }
                uint magic = BitConverter.ToUInt32(bytes, 0);
                const uint DXBC = 0x43425844;
                const uint DXIL = 0x4C495844;
                if (magic != DXBC && magic != DXIL)
                {
                    var r = MessageBox.Show("File does not start with DXBC/DXIL magic. Import anyway?",
                        "Unrecognised CSO", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    if (r != DialogResult.Yes) return;
                }

                int oldSize = (int)s.Size;
                s.Binary = bytes;
                s.Size = (uint)bytes.Length;
                s.BinaryDirty = true;
                // Keep original metadata block — game may crash if the new CSO's
                // resource layout differs from the original.

                LoadAwcShader(s);
                StatusLabel.Text = "Imported " + s.Name + " (" + oldSize + " -> " + bytes.Length + " bytes)";
            }
        }

        private void ExportAllMenuItem_Click(object sender, EventArgs e)
        {
            if (AwcShader == null) return;
            using (var fbd = new FolderBrowserDialog())
            {
                if (fbd.ShowDialog() != DialogResult.OK) return;
                int count = 0;
                foreach (var s in AwcShader.AllShaders())
                {
                    string sub = s.StageName.ToLowerInvariant();
                    string dir = Path.Combine(fbd.SelectedPath, sub);
                    Directory.CreateDirectory(dir);
                    string path = Path.Combine(dir, SafeFileName(s.Name) + ".cso");
                    File.WriteAllBytes(path, s.Binary ?? Array.Empty<byte>());
                    count++;
                }
                StatusLabel.Text = "Exported " + count + " shaders to " + fbd.SelectedPath;
            }
        }

        private static string SafeFileName(string name)
        {
            if (string.IsNullOrEmpty(name)) return "shader";
            var invalid = Path.GetInvalidFileNameChars();
            var sb = new StringBuilder(name.Length);
            foreach (var c in name) sb.Append(Array.IndexOf(invalid, c) >= 0 ? '_' : c);
            return sb.ToString();
        }

        // ---------- AWC: save ----------

        private void SaveMenuItem_Click(object sender, EventArgs e)
        {
            if (AwcShader == null) return;
            if (!IsEditable)
            {
                MessageBox.Show("Enable Edit Mode in RPF Explorer to save AWC files back to the archive.",
                    "Edit Mode required", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (rpfFileEntry == null)
            {
                SaveAsMenuItem_Click(sender, e);
                return;
            }

            try
            {
                if (!(exploreForm?.EnsureRpfValidEncryption(rpfFileEntry.File) ?? false)) return;

                byte[] data = AwcShader.Save();
                var newentry = RpfFile.CreateFile(rpfFileEntry.Parent, rpfFileEntry.Name, data);
                rpfFileEntry = newentry;
                AwcShader.FileEntry = newentry;

                exploreForm?.RefreshMainListViewInvoke();
                StatusLabel.Text = "Saved " + rpfFileEntry.Name + " (" + data.Length + " bytes)";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Save failed: " + ex.Message);
            }
        }

        private void SaveAsMenuItem_Click(object sender, EventArgs e)
        {
            if (AwcShader == null) return;
            using (var sfd = new SaveFileDialog())
            {
                sfd.Filter = "AWC Shader Library (*.awc)|*.awc|All files (*.*)|*.*";
                sfd.FileName = fileName ?? "shader.awc";
                if (sfd.ShowDialog() != DialogResult.OK) return;
                byte[] data = AwcShader.Save();
                File.WriteAllBytes(sfd.FileName, data);
                StatusLabel.Text = "Saved " + Path.GetFileName(sfd.FileName) + " (" + data.Length + " bytes)";
            }
        }
    }
}
