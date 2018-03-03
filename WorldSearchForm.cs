using CodeWalker.GameFiles;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CodeWalker
{
    public partial class WorldSearchForm : Form
    {
        private WorldForm WorldForm;

        private volatile bool AbortOperation = false;

        private List<Archetype> ArchetypeResults = new List<Archetype>();
        private List<YmapEntityDef> EntityResults = new List<YmapEntityDef>();

        public WorldSearchForm(WorldForm worldForm)
        {
            WorldForm = worldForm;
            InitializeComponent();
        }

        private void WorldSearchForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            WorldForm.OnSearchFormClosed();
        }

        private void ArchetypeSearchTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)13)
            {
                ArchetypeSearchButton_Click(sender, e);
                e.Handled = true;
            }
        }

        private void ArchetypeSearchButton_Click(object sender, EventArgs e)
        {
            var s = ArchetypeSearchTextBox.Text;
            var loadedOnly = false;// ArchetypeSearchLoadedOnlyCheckBox.Checked; //NOT WORKING...

            var gfc = WorldForm.GameFileCache;
            if (!gfc.IsInited)
            {
                MessageBox.Show("Please wait for CodeWalker to initialise.");
                return;
            }
            if (s.Length == 0)
            {
                MessageBox.Show("Please enter a search term.");
                return;
            }
            if (s.Length < 2)
            {
                MessageBox.Show("You don't really want to search for that do you?");
                return;
            }

            ArchetypeSearchTextBox.Enabled = false;
            ArchetypeSearchButton.Enabled = false;
            ArchetypeSearchAbortButton.Enabled = true;
            ArchetypeSearchExportResultsButton.Enabled = false;
            AbortOperation = false;
            ArchetypeResults.Clear();
            ArchetypeResultsListView.VirtualListSize = 0;

            s = s.ToLowerInvariant();

            Task.Run(() =>
            {

                var rpfman = gfc.RpfMan;
                var rpflist = loadedOnly ? gfc.ActiveMapRpfFiles.Values.ToList() : rpfman.AllRpfs;
                var results = new List<Archetype>();

                foreach (var rpf in rpflist)
                {
                    foreach (var entry in rpf.AllEntries)
                    {
                        try
                        {
                            if (AbortOperation)
                            {
                                ArchetypeSearchUpdateStatus("Search aborted!");
                                ArchetypeSearchComplete();
                                return;
                            }
                            if (entry.NameLower.EndsWith(".ytyp"))
                            {
                                ArchetypeSearchUpdateStatus(entry.Path);

                                YtypFile ytyp = rpfman.GetFile<YtypFile>(entry);
                                if (ytyp == null) continue;
                                if (ytyp.AllArchetypes == null) continue;

                                foreach (var arch in ytyp.AllArchetypes)
                                {
                                    if (arch.Name.ToLowerInvariant().Contains(s)
                                        || arch.AssetName.ToLowerInvariant().Contains(s))
                                    {
                                        ArchetypeSearchAddResult(arch);
                                        results.Add(arch);
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            ArchetypeSearchUpdateStatus(ex.Message);
                        }
                    }
                }

                ArchetypeSearchUpdateStatus("Search complete. " + results.Count.ToString() + " archetypes found.");
                ArchetypeSearchComplete();
            });

        }

        private void ArchetypeSearchUpdateStatus(string text)
        {
            try
            {
                if (InvokeRequired)
                {
                    Invoke(new Action(() => { ArchetypeSearchUpdateStatus(text); }));
                }
                else
                {
                    ArchetypeSearchStatusLabel.Text = text;
                }
            }
            catch { }
        }

        private void ArchetypeSearchAddResult(Archetype arch)
        {
            try
            {
                if (InvokeRequired)
                {
                    BeginInvoke(new Action(() => { ArchetypeSearchAddResult(arch); }));
                }
                else
                {
                    ArchetypeResults.Add(arch);
                    ArchetypeResultsListView.VirtualListSize = ArchetypeResults.Count;
                }
            }
            catch { }
        }

        private void ArchetypeSearchComplete()
        {
            try
            {
                if (InvokeRequired)
                {
                    Invoke(new Action(() => { ArchetypeSearchComplete(); }));
                }
                else
                {
                    ArchetypeSearchTextBox.Enabled = true;
                    ArchetypeSearchButton.Enabled = true;
                    ArchetypeSearchAbortButton.Enabled = false;
                    ArchetypeSearchExportResultsButton.Enabled = true;
                }
            }
            catch { }
        }

        private void ArchetypeSearchAbortButton_Click(object sender, EventArgs e)
        {
            AbortOperation = true;
        }

        private void ArchetypeSearchExportResultsButton_Click(object sender, EventArgs e)
        {
            if (ArchetypeResults.Count == 0)
            {
                MessageBox.Show("Nothing to export!");
                return;
            }

            SaveFileDialog.FileName = "Archetypes_" + ArchetypeSearchTextBox.Text;
            if (SaveFileDialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            string fname = SaveFileDialog.FileName;

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Name, AssetName, YtypFile");
            foreach (var arch in ArchetypeResults)
            {
                sb.AppendLine(string.Format("{0}, {1}, {2}", arch.Name, arch.AssetName, arch.Ytyp?.RpfFileEntry?.Path ?? ""));
            }

            File.WriteAllText(fname, sb.ToString());
        }

        private void ArchetypeResultsListView_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            if (e.ItemIndex < ArchetypeResults.Count)
            {
                var arch = ArchetypeResults[e.ItemIndex];
                var li = new ListViewItem(new[] { arch.Name, arch.Ytyp?.RpfFileEntry?.Path ?? "" });
                li.Tag = arch;
                e.Item = li;
            }
            else
            {
                e.Item = new ListViewItem("Error retrieving Archetype! Please tell dexyfex");
            }
        }

        private void ArchetypeResultsListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ArchetypeResultsListView.SelectedIndices.Count == 0)
            {
                ArchetypeResultPanel.Enabled = false;
                ArchetypeResultNameTextBox.Text = "";
                ArchetypeResultYtypTextBox.Text = "";
                ArchetypeResultPropertyGrid.SelectedObject = null;
            }
            else
            {
                var li = ArchetypeResultsListView.SelectedIndices[0];
                if (li < ArchetypeResults.Count)
                {
                    var arch = ArchetypeResults[li];
                    ArchetypeResultPanel.Enabled = true;
                    ArchetypeResultNameTextBox.Text = arch.Name;
                    ArchetypeResultYtypTextBox.Text = arch.Ytyp?.RpfFileEntry?.Path ?? "";
                    ArchetypeResultPropertyGrid.SelectedObject = arch;
                }
            }
        }

        private void ArchetypeResultsListView_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (ArchetypeResultsListView.SelectedIndices.Count > 0)
            {
                ArchetypeResultViewModelButton_Click(sender, e);
            }
        }

        private void ArchetypeResultFindEntitiesButton_Click(object sender, EventArgs e)
        {
            MainTabControl.SelectedTab = EntitySearchTabPage;
            EntitySearchTextBox.Text = ArchetypeResultNameTextBox.Text;
            EntitySearchButton_Click(sender, e);
        }

        private void ArchetypeResultViewModelButton_Click(object sender, EventArgs e)
        {
            WorldForm.ShowModel(ArchetypeResultNameTextBox.Text);
        }

        private void EntitySearchTextBox_TextChanged(object sender, EventArgs e)
        {
            //JenkHash h = new JenkHash(EntitySearchTextBox.Text, JenkHashInputEncoding.UTF8);
            //EntitySearchHashLabel.Text = "Hash: " + h.HashUint.ToString() + " (" + h.HashHex + ")";
        }

        private void EntitySearchTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)13)
            {
                EntitySearchButton_Click(sender, e);
                e.Handled = true;
            }
        }

        private void EntitySearchButton_Click(object sender, EventArgs e)
        {
            var s = EntitySearchTextBox.Text;
            var loadedOnly = EntitySearchLoadedOnlyCheckBox.Checked;

            var gfc = WorldForm.GameFileCache;
            if (!gfc.IsInited)
            {
                MessageBox.Show("Please wait for CodeWalker to initialise.");
                return;
            }
            if (s.Length == 0)
            {
                MessageBox.Show("Please enter a search term.");
                return;
            }
            if (s.Length < 2)
            {
                MessageBox.Show("You don't really want to search for that do you?");
                return;
            }

            EntitySearchTextBox.Enabled = false;
            EntitySearchButton.Enabled = false;
            EntitySearchAbortButton.Enabled = true;
            EntitySearchLoadedOnlyCheckBox.Enabled = false;
            EntitySearchExportResultsButton.Enabled = false;
            AbortOperation = false;
            EntityResults.Clear();
            EntityResultsListView.VirtualListSize = 0;

            s = s.ToLowerInvariant();
            //var h = JenkHash.GenHash(s, JenkHashInputEncoding.UTF8);

            Task.Run(() =>
            {

                var rpfman = gfc.RpfMan;
                var rpflist = loadedOnly ? gfc.ActiveMapRpfFiles.Values.ToList() : rpfman.AllRpfs;
                var results = new List<YmapEntityDef>();

                foreach (var rpf in rpflist)
                {
                    foreach (var entry in rpf.AllEntries)
                    {
                        try
                        {
                            if (AbortOperation)
                            {
                                EntitySearchUpdateStatus("Search aborted!");
                                EntitySearchComplete();
                                return;
                            }
                            if (entry.NameLower.EndsWith(".ymap"))
                            {
                                EntitySearchUpdateStatus(entry.Path);

                                YmapFile ymap = rpfman.GetFile<YmapFile>(entry);
                                if (ymap == null) continue;
                                if (ymap.AllEntities == null) continue;

                                foreach (var ent in ymap.AllEntities)
                                {
                                    //if (ent._CEntityDef.archetypeName.Hash == h)
                                    if (ent.Name.ToLowerInvariant().Contains(s))
                                    {
                                        EntitySearchAddResult(ent);
                                        results.Add(ent);
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            EntitySearchUpdateStatus(ex.Message);
                        }
                    }
                }

                EntitySearchUpdateStatus("Search complete. " + results.Count.ToString() + " entities found.");
                EntitySearchComplete();
            });
        }

        private void EntitySearchUpdateStatus(string text)
        {
            try
            {
                if (InvokeRequired)
                {
                    Invoke(new Action(() => { EntitySearchUpdateStatus(text); }));
                }
                else
                {
                    EntitySearchStatusLabel.Text = text;
                }
            }
            catch { }
        }

        private void EntitySearchAddResult(YmapEntityDef ent)
        {
            try
            {
                if (InvokeRequired)
                {
                    BeginInvoke(new Action(() => { EntitySearchAddResult(ent); }));
                }
                else
                {
                    EntityResults.Add(ent);
                    EntityResultsListView.VirtualListSize = EntityResults.Count;
                }
            }
            catch { }
        }

        private void EntitySearchComplete()
        {
            try
            {
                if (InvokeRequired)
                {
                    Invoke(new Action(() => { EntitySearchComplete(); }));
                }
                else
                {
                    EntitySearchTextBox.Enabled = true;
                    EntitySearchButton.Enabled = true;
                    EntitySearchAbortButton.Enabled = false;
                    EntitySearchLoadedOnlyCheckBox.Enabled = true;
                    EntitySearchExportResultsButton.Enabled = true;
                }
            }
            catch { }
        }

        private void EntitySearchAbortButton_Click(object sender, EventArgs e)
        {
            AbortOperation = true;
        }

        private void EntitySearchExportResultsButton_Click(object sender, EventArgs e)
        {
            if (EntityResults.Count == 0)
            {
                MessageBox.Show("Nothing to export!");
                return;
            }

            SaveFileDialog.FileName = "Entities_" + EntitySearchTextBox.Text;
            if (SaveFileDialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            string fname = SaveFileDialog.FileName;

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("ArchetypeName, PositionX, PositionY, PositionZ, RotationX, RotationY, RotationZ, RotationW, YmapFile");
            foreach (var ent in EntityResults)
            {
                sb.AppendLine(string.Format("{0}, {1}, {2}, {3}", ent.Name, FloatUtil.GetVector3String(ent._CEntityDef.position), FloatUtil.GetVector4String(ent._CEntityDef.rotation), ent.Ymap?.RpfFileEntry?.Path ?? ""));
            }

            File.WriteAllText(fname, sb.ToString());
        }

        private void EntityResultsListView_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            if (e.ItemIndex < EntityResults.Count)
            {
                var ent = EntityResults[e.ItemIndex];
                var li = new ListViewItem(new[] { ent.Name, ent.Ymap?.RpfFileEntry?.Path ?? "" });
                li.Tag = ent;
                e.Item = li;
            }
            else
            {
                e.Item = new ListViewItem("Error retrieving YmapEntityDef! Please tell dexyfex");
            }
        }

        private void EntityResultsListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (EntityResultsListView.SelectedIndices.Count == 0)
            {
                EntityResultPanel.Enabled = false;
                EntityResultNameTextBox.Text = "";
                EntityResultYmapTextBox.Text = "";
                EntityResultPropertyGrid.SelectedObject = null;
            }
            else
            {
                var li = EntityResultsListView.SelectedIndices[0];
                if (li < EntityResults.Count)
                {
                    var ent = EntityResults[li];
                    EntityResultPanel.Enabled = true;
                    EntityResultNameTextBox.Text = ent.Name;
                    EntityResultYmapTextBox.Text = ent.Ymap?.RpfFileEntry?.Path ?? "";
                    EntityResultPropertyGrid.SelectedObject = ent;
                }
            }
        }

        private void EntityResultsListView_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (EntityResultsListView.SelectedIndices.Count > 0)
            {
                EntityResultGoToButton_Click(sender, e);
            }
        }

        private void EntityResultGoToButton_Click(object sender, EventArgs e)
        {
            if (EntityResultsListView.SelectedIndices.Count > 0)
            {
                var li = EntityResultsListView.SelectedIndices[0];
                if (li < EntityResults.Count)
                {
                    var ent = EntityResults[li];
                    if (ent.Archetype == null)
                    {
                        var gfc = WorldForm.GameFileCache;
                        ent.Archetype = gfc.GetArchetype(ent._CEntityDef.archetypeName);
                    }
                    WorldForm.GoToEntity(ent);
                }
            }
        }

        private void EntityResultViewModelButton_Click(object sender, EventArgs e)
        {
            WorldForm.ShowModel(EntityResultNameTextBox.Text);
        }
    }
}
